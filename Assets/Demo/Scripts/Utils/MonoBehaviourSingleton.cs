using System;
using UnityEngine;

namespace Utils.Singletons
{
	public interface IKeepAliveMonoBehaviourSingleton
	{
	};

	public interface IAlwaysAccessibleOnQuit
	{
	};

	public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static bool isQuitting = false;
		public static bool isInitialized = false;

		public static T instance
		{
			get
			{
				if (isQuitting)
				{
					if (m_instance is IAlwaysAccessibleOnQuit)
					{
						return m_instance;
					}

					return null;
				}

				return m_instance ?? CreateInstance();
			}
		}

		private static T m_instance;

		protected virtual void Awake()
		{
			if (m_instance == null || m_instance == this)
			{
				m_instance = this as T;

				if (this is IKeepAliveMonoBehaviourSingleton)
				{
					DontDestroyOnLoad(gameObject);
				}

				try
				{
					if (!isInitialized)
					{
						Initialization();
					}
				}
				catch (Exception E)
				{
					Debug.LogException(E);
				}
				finally
				{
					isInitialized = true;
				}
			}
			else
			{
				Destroy(gameObject);
			}
		}

		private void OnDestroy()
		{
			try
			{
				if (this == m_instance)
				{
					try
					{
						Finalization();
					}
					catch (Exception E)
					{
						Debug.LogException(E);
					}
					finally
					{
						isInitialized = false;
					}
				}
			}
			catch (Exception E)
			{
				Debug.LogException(E);
			}

			if (this == m_instance)
			{
				m_instance = null;
			}
		}

		protected virtual void OnApplicationQuit()
		{
			isQuitting = true;
		}

		protected virtual void Initialization()
		{
		}

		protected virtual void Finalization()
		{
		}

		private static T CreateInstance()
		{
			if (m_instance == null)
			{
				try
				{
					m_instance = FindObjectOfType<T>() as T;
				}
				catch (Exception E)
				{
					Debug.Log(E);
				}
				finally
				{
					if (Application.isPlaying)
					{
						if (m_instance != null)
						{
							var gameObject = m_instance.gameObject;
							gameObject.SetActive(false);
							Destroy(gameObject);
							m_instance = null;
						}

						var go = new GameObject(typeof(T).ToString());

						go.SetActive(false);
						m_instance = go.AddComponent<T>();
						go.SetActive(true);
					}
				}
			}

			return m_instance;
		}

		public static bool hasInstance => (!isQuitting || (m_instance is IAlwaysAccessibleOnQuit)) && m_instance != null;
	}
}