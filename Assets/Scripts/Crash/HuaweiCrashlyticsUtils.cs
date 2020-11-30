using System;
using HuaweiMobileServices.Crash;
using UnityEngine;

namespace Services.Debug
{
	public class HuaweiCrashlyticsUtils : ICrashlyticsUtils
	{
		private IAGConnectCrash m_crashlytics;

		public void Init(Action<bool> onComplete)
		{
			m_crashlytics = AGConnectCrash.GetInstance();
			m_crashlytics.EnableCrashCollection(true);
			onComplete?.Invoke(true);
		}

		public void SetCustomKey(string key, string value)
		{
			m_crashlytics.SetCustomKey(key, value);
		}

		public void Crash()
		{
			UnityEngine.Diagnostics.Utils.ForceCrash(0);
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