using System;
using HuaweiMobileServices.Crash;

namespace Services.Debug
{
	public class HuaweiCrashlyticsUtils : ICrashlyticsUtils
	{
		private AgConnectCrash m_crashlytics;

		public void Init(Action<bool> onComplete)
		{
			if (AGConnectInstance.GetInstance() == null)
			{
				AGConnectInstance.Initialize();
			}
			m_crashlytics = AgConnectCrash.GetInstance();
			m_crashlytics.EnableCrashCollection(true);
			onComplete?.Invoke(true);
		}

		public void SetCustomKey(string key, string value)
		{
			m_crashlytics.SetCustomKey(key, value);
		}

		public void Crash()
		{
			m_crashlytics.TestIt();
		}

		public void Log(string log)
		{
			m_crashlytics.Log(log);
		}

		public void LogException(Exception exception)
		{
			
		}

		public void SetUserId(string userId)
		{
			m_crashlytics.SetUserId(userId);
		}
	}
}