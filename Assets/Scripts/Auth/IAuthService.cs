
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AuthServices
{
	public class AuthServiceResult
	{
		public bool success { get; private set; }

		public string userName { get; private set; }
		public Texture2D userIcon { get; private set; }

		public Dictionary<string, string> formParams { get; private set; }

		public AuthServiceResult(bool result, string name = null, Texture2D icon = null, Dictionary<string, string> param = null)
		{
			success = result;
			userName = name;
			userIcon = icon;
			formParams = param != null ? new Dictionary<string, string>(param) : new Dictionary<string, string>();
		}
	}

	public interface IAuthService
	{
		void Authenticate(Action<AuthServiceResult> callback);
		void LogOut(Action<bool> callback);

		bool isLoggedIn { get; }
	}
}