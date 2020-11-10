namespace HuaweiConstants
{
    public static class IAP
    {
        public enum IapType
        {
            CONSUMABLE = 0,
            NON_CONSUMABLE = 1,
            SUBSCRIPTION = 2
        }

        #if UNITY_PURCHASING
        public static UnityEngine.Purchasing.ProductType GetProductType(IapType iapType)
        {
	        switch (iapType)
	        {
		        case IapType.CONSUMABLE:
			        return UnityEngine.Purchasing.ProductType.Consumable;
		        case IapType.NON_CONSUMABLE:
			        return UnityEngine.Purchasing.ProductType.NonConsumable;
		        case IapType.SUBSCRIPTION:
			        return UnityEngine.Purchasing.ProductType.Subscription;
	        }

	        return UnityEngine.Purchasing.ProductType.Consumable;
        }
        #endif
    }

}