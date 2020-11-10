using System;

namespace Services.Debug
{
	public interface ICrashlyticsUtils
	{
		void Init(Action<bool> onComplete);
		void SetCustomKey(string key, string value);
		void Crash();
		void Log(string log);
		void LogException(Exception exception);
		void SetUserId(string userId);
	}
}