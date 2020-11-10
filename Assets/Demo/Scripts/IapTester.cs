using HmsPlugin;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class IapTester : MonoBehaviour, IStoreListener
{
    [SerializeField] private Transform m_consumablesParent;
    [SerializeField] private Transform m_nonConsumablesParent;
    [SerializeField] private Transform m_subscriptionsParent;
    
    [SerializeField] private GameObject m_iapPrefab;
    
    [SerializeField] private string[] m_subscriptions;
    [SerializeField] private string[] m_consumables;
    [SerializeField] private string[] m_nonConsumables;

    private IStoreController m_controller;
    
    private void Start()
    {
        var module = HuaweiPurchasingModule.Instance();
        var builder = ConfigurationBuilder.Instance(module);
        
        AddProducts(builder, m_subscriptions, ProductType.Subscription);
        AddProducts(builder, m_consumables, ProductType.Consumable);
        AddProducts(builder, m_nonConsumables, ProductType.NonConsumable);
		
        UnityPurchasing.Initialize(this, builder);
    }

    private void AddProducts(ConfigurationBuilder builder, string[] productIds, ProductType type)
    {
        foreach (var id in productIds)
        {
            builder.AddProduct(id, type, new IDs
            {
#if UNITY_IOS
				{ id, AppleAppStore.Name }
#elif HUAWEI_APPGALLERY
                { id, HuaweiStore.Name }
#else
				{ id, GooglePlay.Name }
#endif
            });
        }
    }

    private void CreatePrefabs(Transform parent, string[] iaps)
    {
        foreach (var iap in iaps)
        {
            var iapGo = Instantiate(m_iapPrefab, parent);
            iapGo.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                m_controller.InitiatePurchase(iap);
            });
            
            decimal price = 0;
            var currency = "USD";
            if (GetProductPrice(iap, ref price, ref currency))
            {
                iapGo.GetComponentInChildren<Text>().text = price + currency;
            } 
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEventArgs)
    {
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_controller = controller;
        
        CreatePrefabs(m_nonConsumablesParent, m_consumables);
        CreatePrefabs(m_consumablesParent, m_nonConsumables);
        CreatePrefabs(m_subscriptionsParent, m_subscriptions);
    }

    #region Products

    public bool GetProductCurrencyCode(string productId, ref string currencyCode)
    {
        var product = GetProduct(productId);
        if (product != null)
        {
            currencyCode = product.metadata.isoCurrencyCode;
            return true;
        }
        return false;
    }

    public string GetProductLocalizedName(string productId)
    {
        var product = GetProduct(productId);
        if (product != null)
        {
            return product.metadata.localizedTitle;
        }
        return "Unknown";
    }

    public bool GetProductPrice(string productId, ref decimal price, ref string isoCurrencyCode)
    {
        var product = GetProduct(productId);
        if (product != null)
        {
            price = product.metadata.localizedPrice;
            isoCurrencyCode = product.metadata.isoCurrencyCode;

            return true;
        }
        return false;
    }

    private Product GetProduct(string productId)
    {
        var products = m_controller.products.all;
        if (products != null)
        {
            foreach (var product in products)
            {
                if (product.definition.id.Equals(productId) && product.availableToPurchase)
                {
                    return product;
                }
            }
        }

        return null;
    }

    #endregion
}
