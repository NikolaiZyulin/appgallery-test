using System;
using System.Collections.Generic;
using HuaweiMobileServices.Push;
using UnityEngine;

namespace SubModules.GameCore.Services
{
	public class HuaweiPushNotificationService : BasePushNotificationService, IPushListener
	{
		public override void Init()
		{
			PushManager.Listener = this;
			var token = PushManager.Token;
			Debug.Log($"[HMS] Push token from GetToken is {token}");
			if (!string.IsNullOrEmpty(token))
			{
				m_listener?.SendRegistrationIdHandler(token);
			}
		}

		public void OnNewToken(string token)
		{
			Debug.Log($"[HMS] Push token from OnNewToken is {token}");
			if (!string.IsNullOrEmpty(token))
			{
				m_listener?.SendRegistrationIdHandler(token);
			}
		}

		public void OnTokenError(Exception e)
		{
			Debug.Log("[HMS] Error asking for Push token");
			Debug.Log(e.StackTrace);
		}

		public void OnMessageReceived(RemoteMessage remoteMessage)
		{
			var receivedPushNotifications = new List<ReceivedPushNotification>();
			var notification = remoteMessage.GetNotification;
			receivedPushNotifications.Add(new ReceivedPushNotification()
			{
				title = notification.Title,
				text = notification.Body
			});
			m_listener?.NotificationsReceivedHandler(receivedPushNotifications);
		}

		public override void Destroy()
		{
			PushManager.Listener = null;
		}
	}
}