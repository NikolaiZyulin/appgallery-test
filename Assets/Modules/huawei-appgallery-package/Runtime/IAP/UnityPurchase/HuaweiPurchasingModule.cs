#if UNITY_PURCHASING
using UnityEngine.Purchasing.Extension;

namespace HmsPlugin
{
	public class HuaweiPurchasingModule : AbstractPurchasingModule
	{
		public static HuaweiPurchasingModule Instance()
		{
			if (m_instance != null)
			{
				return m_instance;
			}
			
			m_instance = new HuaweiPurchasingModule();
			return m_instance;
		}

		private static HuaweiPurchasingModule m_instance;

		public override void Configure()
		{
			RegisterStore(HuaweiStore.Name, new HuaweiStore());
		}
	}
}
#endif