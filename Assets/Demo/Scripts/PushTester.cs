using System.Collections.Generic;
using SubModules.GameCore.Services;
using UnityEngine;

public class PushTester : MonoBehaviour, IPushNotificationListener
{
	private IPushNotificationService m_notificationService;

	private void Awake()
	{
		m_notificationService = new HuaweiPushNotificationService();
		m_notificationService.SetListener(this);
		m_notificationService.Init();
	}

	public void SendRegistrationIdHandler(string regId)
	{
		Debug.Log($"PushTester: {regId}");
	}

	public void NotificationClickedHandler(ReceivedPushNotification notification)
	{
		Debug.Log($"PushTester: {notification.title} {notification.text}");
	}

	public void NotificationsReceivedHandler(IList<ReceivedPushNotification> receivedNotifications)
	{
		foreach (var receivedNotification in receivedNotifications)
		{
			Debug.Log($"PushTester: {receivedNotification.title} {receivedNotification.text}");
		}
	}

	private void OnDestroy()
	{
		m_notificationService?.Destroy();
	}
}
