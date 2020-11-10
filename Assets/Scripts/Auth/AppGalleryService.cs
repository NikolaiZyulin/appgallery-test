using System;
using UnityEngine;
using Utils.TimeManagement;
using HuaweiMobileServices.Id;

namespace AuthServices
{
	public class AppGalleryService : IAuthService
	{
		private static bool m_initialized;

		public bool isLoggedIn => m_isLoggedIn;

		private HuaweiIdAuthService m_authService;
		private bool m_isLoggedIn;

		public void Authenticate(Action<AuthServiceResult> callback)
		{
			if (!m_initialized)
			{
				var authParams = new HuaweiIdAuthParamsHelper(HuaweiIdAuthParams.DEFAULT_AUTH_REQUEST_PARAM_GAME).SetIdToken().CreateParams();
				m_authService = HuaweiIdAuthManager.GetService(authParams);
				
				m_initialized = true;

			}
			
			m_authService.SignOut();
			m_authService.StartSignIn((authId) =>
			{
				TimeController.instance.CallFromMainThread(() =>
				{
					m_isLoggedIn = true;
					callback?.Invoke(new AuthServiceResult(true, authId.DisplayName));
				});
			}, (error) =>
			{
				TimeController.instance.CallFromMainThread(() =>
				{
					Debug.Log($"AppGalleryService -> Auth failed! {error.Message}!");
					callback?.Invoke(new AuthServiceResult(false));
				});
			});
		}

		public void LogOut(Action<bool> callback)
		{
			if (isLoggedIn)
			{
				Debug.Log("AppGalleryService -> Performing sign out");
				m_authService?.SignOut();
				m_isLoggedIn = false;
				callback?.Invoke(true);
			}
			else
			{
				callback?.Invoke(false);
			}
		}
	}
}