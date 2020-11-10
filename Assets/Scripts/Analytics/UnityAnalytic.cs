using System.Collections.Generic;
using UnityAnalytics = UnityEngine.Analytics.Analytics;

namespace Analytics
{
	public class UnityAnalytic : IAnalytic
	{
		public void Init()
		{
			
		}

		public void LogEvent(string action)
		{
			UnityAnalytics.CustomEvent(action);
		}

		public void LogEvent(string action, Dictionary<string, string> parameters)
		{
			var objectParameters = new  Dictionary<string, object>(parameters.Count);
			foreach (var parameter in parameters)
			{
				objectParameters.Add(parameter.Key, parameter.Value);
			}
			UnityAnalytics.CustomEvent(action, objectParameters);
		}

		public void SetUserID(string userId)
		{
			if (!string.IsNullOrEmpty(userId))
			{
				UnityAnalytics.SetUserId(userId);
			}
		}
	}
}