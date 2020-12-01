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
				Debug.Log("AppGalleryService -> CreateAuthParams");
				var authParams = new HuaweiIdAuthParamsHelper(HuaweiIdAuthParams.DEFAULT_AUTH_REQUEST_PARAM_GAME).SetIdToken().SetAccessToken().CreateParams();
				Debug.Log("AppGalleryService -> GetService");
				m_authService = HuaweiIdAuthManager.GetService(authParams);

				m_initialized = true;
			}

			Debug.Log("AppGalleryService -> Start SignOut");
			var signOut = m_authService.SignOut();
			signOut.AddOnSuccessListener(success =>
			{
				Debug.Log($"AppGalleryService -> SignOut Success");
				SignIn(callback);
			});
			signOut.AddOnFailureListener(error =>
			{
				Debug.Log($"AppGalleryService -> SignOut Error {error.Message}");
				SignIn(callback);
			});
		}

		private void SignIn(Action<AuthServiceResult> callback)
		{
			Debug.Log("AppGalleryService -> Start SignIn");
			m_authService.StartSignIn(authId =>
			{
				TimeController.instance.CallFromMainThread(() =>
				{
					Debug.Log($"AppGalleryService -> SignIn Success {authId.DisplayName}");
					m_isLoggedIn = true;
					callback?.Invoke(new AuthServiceResult(true, authId.DisplayName));
				});
			}, error =>
			{
				TimeController.instance.CallFromMainThread(() =>
				{
					Debug.Log($"AppGalleryService -> SignIn Error {error.Message}");
					callback?.Invoke(new AuthServiceResult(false));
				});
			});
		}

		public void LogOut(Action<bool> callback)
		{
			if (isLoggedIn && m_authService != null)
			{
				Debug.Log("AppGalleryService -> Start SignOut");
				var signOut = m_authService.SignOut();
				signOut.AddOnSuccessListener(success =>
				{
					Debug.Log("AppGalleryService -> SignOut Success");
					m_isLoggedIn = false;
					callback?.Invoke(true);
				});
				signOut.AddOnFailureListener(error =>
				{
					Debug.Log($"AppGalleryService -> SignOut Error {error.Message}");
					callback?.Invoke(false);
				});
			}
			else
			{
				callback?.Invoke(false);
			}
		}
	}
}