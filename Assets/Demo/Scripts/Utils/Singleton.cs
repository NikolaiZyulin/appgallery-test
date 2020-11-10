using UnityEngine;

namespace Utils.Singletons
{
	public abstract class Singleton<T> where T : new()
	{
		public static bool hasInstance => m_instance != null;

		public static T instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = new T();
				}

				return m_instance;
			}
		}

		private static T m_instance;

		public Singleton()
		{
			if (m_instance != null)
			{
#if UNITY_EDITOR
				Debug.LogError($"Singleton {typeof(T).Name} already have instance!");
#endif
				return;
			}
		}
	}
}