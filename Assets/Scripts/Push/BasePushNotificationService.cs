namespace SubModules.GameCore.Services
{
	public abstract class BasePushNotificationService: IPushNotificationService
	{
		public bool initialized { get; protected set; }

		protected IPushNotificationListener m_listener;
		
		public abstract void Init();

		public void SetListener(IPushNotificationListener listener)
		{
			m_listener = listener;
		}
		
		public abstract void Destroy();
	}
}