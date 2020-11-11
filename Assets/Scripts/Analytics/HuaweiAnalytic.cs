using System.Collections.Generic;
using HuaweiMobileServices.Analytics;
using HuaweiMobileServices.Utils;

namespace Analytics
{
	public class HuaweiAnalytic : IAnalytic
	{
		private HiAnalyticsInstance m_analytics;

		public void Init()
		{
			HiAnalyticsTools.EnableLog();
			m_analytics = HiAnalytics.GetInstance();
		}

		public void LogEvent(string action)
		{
			m_analytics.OnEvent(action, new Bundle());
		}

		public void LogEvent(string action, Dictionary<string, string> parameters)
		{
			var bundle = new Bundle();
			foreach (var parameter in parameters)
			{
				bundle.PutString(parameter.Key, parameter.Value);
			}
			m_analytics.OnEvent(action, bundle);
		}

		public void SetUserID(string userId)
		{
			m_analytics.SetUserId(userId);
		}
	}
}