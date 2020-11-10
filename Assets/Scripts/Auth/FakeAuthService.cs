using System;

namespace AuthServices
{
	public class FakeAuthService: IAuthService
	{
		public bool isLoggedIn { get; private set; }
		
		public void Authenticate(Action<AuthServiceResult> callback)
		{
			callback?.Invoke(new AuthServiceResult(true));
			isLoggedIn = true;
		}

		public void LogOut(Action<bool> callback)
		{
			callback?.Invoke(true);
			isLoggedIn = false;
		}
	}
}