using System;

public class FakeAdService : IAdService
{
	public event Action<bool> onVideoRewarded;
	public event Action<bool> onInterstitial;
	
	public bool initialized { get; private set; }

	public void Init()
	{
		initialized = true;
	}

	public void Clear()
	{
		
	}

	public void SetUserData(string userId)
	{
		
	}

	public void RequestInterstitial()
	{
		
	}

	public void ShowInterstitial()
	{
		onInterstitial?.Invoke(true);
	}

	public bool NeedRequestInterstitial()
	{
		return false;
	}

	public bool IsInterstitialAvailable()
	{
		return true;
	}

	public void RequestVideo()
	{
		
	}

	public void ShowVideo()
	{
		onVideoRewarded?.Invoke(true);
	}

	public bool NeedRequestVideo()
	{
		return false;
	}

	public bool IsVideoAvailable()
	{
		return true;
	}
}