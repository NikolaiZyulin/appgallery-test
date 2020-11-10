using UnityEngine;

namespace SubModules.GameCore.Services
{
	public class FakePushNotificationService: BasePushNotificationService
	{
		public override void Init()
		{
			Debug.Log("FakePushNotificationService initialized");
			initialized = true;
		}

		public override void Destroy()
		{
			Debug.Log("FakePushNotificationService destroyed");
			initialized = false;
		}
	}
}