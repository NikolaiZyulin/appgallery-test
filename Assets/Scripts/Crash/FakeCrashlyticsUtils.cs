using System;

namespace Services.Debug
{
	public class FakeCrashlyticsUtils : ICrashlyticsUtils
	{
		public void Init(Action<bool> onComplete)
		{
			onComplete?.Invoke(false);
		}

		public void SetCustomKey(string key, string value)
		{
			
		}

		public void Crash()
		{
			
		}

		public void Log(string log)
		{
			
		}

		public void LogException(Exception exception)
		{
			
		}

		public void SetUserId(string userId)
		{
			
		}
	}
}