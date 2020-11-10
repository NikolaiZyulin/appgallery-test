using System.Collections.Generic;

namespace SubModules.GameCore.Services
{
	public interface IPushNotificationListener
	{
		void SendRegistrationIdHandler(string regId);
		void NotificationClickedHandler(ReceivedPushNotification notification);
		void NotificationsReceivedHandler(IList<ReceivedPushNotification> receivedNotifications);
	}
}