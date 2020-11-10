using System;
using System.Collections.Generic;
using System.Threading;
using Utils.Singletons;

namespace Utils.TimeManagement
{
	public class TimeController : MonoBehaviourSingleton<TimeController>, IKeepAliveMonoBehaviourSingleton
	{
		private readonly List<Action> m_syncCallbacks = new List<Action>();
		private int m_mainThreadId;

		protected override void Awake()
		{
			base.Awake();

			m_mainThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		public void CallFromMainThread(Action action)
		{
			if (Thread.CurrentThread.ManagedThreadId != m_mainThreadId)
			{
				lock (m_syncCallbacks)
				{
					m_syncCallbacks.Add(action);
				}
			}
			else
			{
				action?.Invoke();
			}
		}

		private void Update()
		{
			lock (m_syncCallbacks)
			{
				if (m_syncCallbacks.Count > 0)
				{
					foreach (var callback in m_syncCallbacks)
					{
						callback?.Invoke();
					}
					m_syncCallbacks.Clear();
				}
			}
		}
	}
}