#if UNITY_PURCHASING
using System.Collections.ObjectModel;
using System.Collections.Generic;
using HuaweiMobileServices.IAP;
using HuaweiMobileServices.Utils;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using System.Linq;
using System.Text;
using HuaweiConstants;
using UnityEngine;


namespace HmsPlugin
{
	public class HuaweiStore : IStore
	{
		private class PurchaseReceipt
		{
			public string json;
			public string signature;
		}

		private class ReceiptPayload
		{
			public string packageName;
			public string productId;
			public string purchaseTime;
			public string purchaseState;
			public string purchaseToken;
		}

		private class SubscriptionPurchaseToken
		{
			public string purchaseToken;
			public string subscriptionId;
		}

		public const string Name = "Huawei";

		private readonly Dictionary<string, InAppPurchaseData> m_purchasedData;
		private readonly Dictionary<string, ProductInfo> m_productsByID;
		private readonly List<ProductInfo> m_productsList;

		private ReadOnlyCollection<ProductDefinition> m_initProductDefinitions;

		private IStoreCallback m_storeEvents;
		private IIapClient m_iapClient;
		private bool m_clientInited;
		private object m_locker;

		public HuaweiStore()
		{
			m_locker = new object();
			m_productsList = new List<ProductInfo>(100);
			m_productsByID = new Dictionary<string, ProductInfo>(100);
			m_purchasedData = new Dictionary<string, InAppPurchaseData>(50);
		}

		#region IStore Callbacks

		void IStore.Initialize(IStoreCallback callback)
		{
			m_storeEvents = callback;

			CreateClient();
		}

		void IStore.RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
		{
			lock (m_locker)
			{
				m_initProductDefinitions = products;
				if (m_clientInited)
				{
					LoadConsumableProducts();
				}
			}
		}

		void IStore.Purchase(ProductDefinition product, string developerPayload)
		{
			if (!m_productsByID.ContainsKey(product.storeSpecificId))
			{
				m_storeEvents.OnPurchaseFailed(new PurchaseFailureDescription(product.id, PurchaseFailureReason.ProductUnavailable, "UnknownProduct"));
				return;
			}

			var productInfo = m_productsByID[product.storeSpecificId];
			var purchaseIntentReq = new PurchaseIntentReq
			{
				PriceType = productInfo.PriceType,
				ProductId = productInfo.ProductId,
				DeveloperPayload = developerPayload
			};

			var task = m_iapClient.CreatePurchaseIntent(purchaseIntentReq);
			task.AddOnSuccessListener((intentResult) => { PurchaseIntentCreated(intentResult, product); });
			task.AddOnFailureListener((exception) =>
			{
				m_storeEvents.OnPurchaseFailed(new PurchaseFailureDescription(product.id, PurchaseFailureReason.Unknown, exception.Message));
			});
		}

		void IStore.FinishTransaction(ProductDefinition product, string transactionId)
		{
			if (m_purchasedData.TryGetValue(product.storeSpecificId, out var data))
			{
				var token = data.PurchaseToken;
				var request = new ConsumeOwnedPurchaseReq {PurchaseToken = token};

				var task = m_iapClient.ConsumeOwnedPurchase(request);
				task.AddOnSuccessListener((result) => { m_purchasedData.Remove(product.storeSpecificId); });
				task.AddOnFailureListener((exception) => { Debug.Log("Consume failed " + exception.Message + " " + exception.StackTrace); });
			}
		}

		#endregion

		private void CreateClient()
		{
			m_iapClient = Iap.GetIapClient();

			var moduleInitTask = m_iapClient.EnvReady;
			moduleInitTask.AddOnSuccessListener(ClientInitSuccess);
			moduleInitTask.AddOnFailureListener(ClientInitFailed);
		}

		private void ClientInitSuccess(EnvReadyResult result)
		{
			lock (m_locker)
			{
				m_clientInited = true;
				if (m_initProductDefinitions != null)
				{
					LoadConsumableProducts();
				}
			}
		}

		private void ClientInitFailed(HMSException exception)
		{
			Debug.Log($"HuaweiStore: ClientInitFailed {exception.Message}");
			m_storeEvents.OnSetupFailed(InitializationFailureReason.PurchasingUnavailable);
		}

		private void LoadConsumableProducts()
		{
			var consumablesIDs = (from definition in m_initProductDefinitions where definition.type == ProductType.Consumable select definition.storeSpecificId)
				.ToList();
			CreateProductRequest(consumablesIDs, IAP.IapType.CONSUMABLE, LoadNonConsumableProducts);
		}

		private void LoadNonConsumableProducts()
		{
			var nonConsumablesIDs = (from definition in m_initProductDefinitions
				where definition.type == ProductType.NonConsumable
				select definition.storeSpecificId).ToList();
			CreateProductRequest(nonConsumablesIDs, IAP.IapType.NON_CONSUMABLE, LoadSubscribeProducts);
		}

		private void LoadSubscribeProducts()
		{
			var nonConsumablesIDs =
				(from definition in m_initProductDefinitions where definition.type == ProductType.Subscription select definition.storeSpecificId).ToList();
			CreateProductRequest(nonConsumablesIDs, IAP.IapType.SUBSCRIPTION, LoadOwnedConsumables);
		}

		private void CreateProductRequest(List<string> consumablesIDs, IAP.IapType type, System.Action onSuccess)
		{
			if (consumablesIDs.Count == 0)
			{
				onSuccess();
				return;
			}

			var productsDataRequest = new ProductInfoReq();
			productsDataRequest.PriceType = (int) type;
			productsDataRequest.ProductIds = consumablesIDs;

			var task = m_iapClient.ObtainProductInfo(productsDataRequest);
			task.AddOnFailureListener(exception =>
			{
				Debug.Log($"HuaweiStore: GetProductsFailure type {type} exception {exception.Message}");
				m_storeEvents.OnSetupFailed(InitializationFailureReason.PurchasingUnavailable);
			});
			task.AddOnSuccessListener((result) =>
			{
				ParseProducts(result, type.ToString());
				onSuccess();
			});
		}

		private void ParseProducts(ProductInfoResult result, string type)
		{
			if (result == null)
			{
				return;
			}

			if (result.ProductInfoList.Count == 0)
			{
				return;
			}

			foreach (var productInfo in result.ProductInfoList)
			{
				m_productsList.Add(productInfo);
				m_productsByID.Add(productInfo.ProductId, productInfo);
			}
		}

		private void LoadOwnedConsumables()
		{
			CreateOwnedPurchaseRequest(IAP.IapType.CONSUMABLE, LoadOwnedNonConsumables);
		}

		private void LoadOwnedNonConsumables()
		{
			CreateOwnedPurchaseRequest(IAP.IapType.NON_CONSUMABLE, LoadOwnedSubscribes);
		}

		private void LoadOwnedSubscribes()
		{
			CreateOwnedPurchaseRequest(IAP.IapType.SUBSCRIPTION, ProductsLoaded);
		}

		private void CreateOwnedPurchaseRequest(IAP.IapType type, System.Action onSuccess)
		{
			var ownedPurchasesReq = new OwnedPurchasesReq {PriceType = (int) type};

			var task = m_iapClient.ObtainOwnedPurchases(ownedPurchasesReq);

			task.AddOnSuccessListener((result) =>
			{
				ParseOwned(result);
				onSuccess();
			});
		}

		private void ParseOwned(OwnedPurchasesResult result)
		{
			if (result?.InAppPurchaseDataList == null)
			{
				return;
			}

			foreach (var inAppPurchaseData in result.InAppPurchaseDataList)
			{
				var inAppPurchaseDataBean = new InAppPurchaseData(inAppPurchaseData);
				m_purchasedData[inAppPurchaseDataBean.ProductId] = inAppPurchaseDataBean;
			}
		}

		private void ProductsLoaded()
		{
			var descList = new List<ProductDescription>(m_productsList.Count);

			foreach (var product in m_productsList)
			{
				var price = product.MicrosPrice * 0.000001f;
				var prodMeta = new ProductMetadata(product.Price, product.ProductName, product.ProductDesc, product.Currency, (decimal) price);

				ProductDescription prodDesc;
				if (m_purchasedData.TryGetValue(product.ProductId, out var purchaseData))
				{
					prodDesc = new ProductDescription(product.ProductId, prodMeta, CreateReceipt(purchaseData), purchaseData.OrderID);
				}
				else
				{
					prodDesc = new ProductDescription(product.ProductId, prodMeta);
				}

				descList.Add(prodDesc);
			}

			m_storeEvents.OnProductsRetrieved(descList);
		}

		private static string CreateReceipt(InAppPurchaseData purchaseData)
		{
			var sb = new StringBuilder(1024);

			sb.Append('{').Append("\"Store\":\"AppGallery\",\"TransactionID\":\"").Append(purchaseData.OrderID).Append("\", \"Payload\":{ ");
			sb.Append("\"product\":\"").Append(purchaseData.ProductId).Append("\"");
			sb.Append('}');
			sb.Append('}');
			return sb.ToString();
		}

		private void PurchaseIntentCreated(PurchaseIntentResult intentResult, ProductDefinition product)
		{
			if (intentResult == null)
			{
				m_storeEvents.OnPurchaseFailed(new PurchaseFailureDescription(product.id, PurchaseFailureReason.Unknown, "IntentIsNull"));
				return;
			}

			var status = intentResult.Status;
			status.StartResolutionForResult((androidIntent) =>
			{
				var purchaseResultInfo = m_iapClient.ParsePurchaseResultInfoFromIntent(androidIntent);
				Debug.Log($"HuaweiStore: PurchaseIntentCreated with Result Code {purchaseResultInfo.ReturnCode}");

				switch (purchaseResultInfo.ReturnCode)
				{
					case OrderStatusCode.ORDER_STATE_SUCCESS:
						var purchaseData = purchaseResultInfo.InAppPurchaseData;
						var data = new InAppPurchaseData(purchaseData);

						if (product.type == ProductType.Subscription)
						{
							var receiptPayload = JsonUtility.FromJson<ReceiptPayload>(purchaseData);
							var subscriptionPurchaseToken = new SubscriptionPurchaseToken()
							{
								purchaseToken = receiptPayload.purchaseToken,
								subscriptionId = data.SubscriptionId
							};
							receiptPayload.purchaseToken = JsonUtility.ToJson(subscriptionPurchaseToken);
							purchaseData = JsonUtility.ToJson(receiptPayload);
						}

						var receipt = new PurchaseReceipt
						{
							signature = purchaseResultInfo.InAppDataSignature,
							json = purchaseData
						};

						m_purchasedData[product.storeSpecificId] = data;
						m_storeEvents.OnPurchaseSucceeded(product.storeSpecificId, JsonUtility.ToJson(receipt), data.OrderID);
						break;
					case OrderStatusCode.ORDER_PRODUCT_OWNED:
						m_storeEvents.OnPurchaseFailed(new PurchaseFailureDescription(product.storeSpecificId, PurchaseFailureReason.DuplicateTransaction,
							purchaseResultInfo.ErrMsg));
						break;
					case OrderStatusCode.ORDER_STATE_CANCEL:
						m_storeEvents.OnPurchaseFailed(new PurchaseFailureDescription(product.storeSpecificId, PurchaseFailureReason.UserCancelled,
							purchaseResultInfo.ErrMsg));
						break;
					default:
						m_storeEvents.OnPurchaseFailed(new PurchaseFailureDescription(product.storeSpecificId, PurchaseFailureReason.Unknown,
							purchaseResultInfo.ErrMsg));
						break;
				}
			}, (exception) =>
			{
				m_storeEvents.OnPurchaseFailed(new PurchaseFailureDescription(product.id, PurchaseFailureReason.Unknown, exception.Message));
			});
		}
	}
}

#endif