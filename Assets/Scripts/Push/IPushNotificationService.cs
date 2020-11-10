namespace SubModules.GameCore.Services
{
	public interface IPushNotificationService
	{
		bool initialized { get; }
		void Init();
		void SetListener(IPushNotificationListener listener);
		void Destroy();
	}
}