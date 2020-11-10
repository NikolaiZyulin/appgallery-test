using System;

public interface IAdService
{
	event Action<bool> onVideoRewarded;
	event Action<bool> onInterstitial;
	
	bool initialized { get; }
	void Init();
	void Clear();

	void SetUserData(string userId);

	void RequestInterstitial();
	void ShowInterstitial();
	bool NeedRequestInterstitial();
	bool IsInterstitialAvailable();
	
	void RequestVideo();
	void ShowVideo();
	bool NeedRequestVideo();
	bool IsVideoAvailable();
}