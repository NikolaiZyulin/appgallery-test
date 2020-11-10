using System;
using HuaweiMobileServices.Ads;
using UnityEngine;
using Utils.TimeManagement;

public static class HuaweiAdsIds
{
#if TEST_BUILD || DEV_BUILD
	public const string InterstitialId = "testb4znbuh3n2";
	public const string VideoId = "testx9dtjwj8hp";
#else
	public const string InterstitialId = "m4pvr4sj1o";
	public const string VideoId = "v913bjzslv";
#endif
}

public class HuaweiAdService : IAdService, IAdListener, IRewardAdStatusListener
{
	public event Action<bool> onVideoRewarded;
	public event Action<bool> onInterstitial;
	
	public bool initialized { get; private set; }

	private RewardAd m_rewardAd;
	private InterstitialAd m_interstitialAd;
	private bool m_rewardedByVideo;

	public void Init()
	{
		HwAds.Init();
		initialized = true;
	}

	public void Clear()
	{
		
	}

	public void SetUserData(string userId)
	{
		int personalizedAd;
		int tagForChildProtection;
		
		if (!string.IsNullOrEmpty(userId))
		{
			personalizedAd = NonPersonalizedAd.ALLOW_ALL;
			tagForChildProtection = TagForChild.TAG_FOR_CHILD_PROTECTION_FALSE;
		}
		else
		{
			personalizedAd = NonPersonalizedAd.ALLOW_NON_PERSONALIZED;
			tagForChildProtection = TagForChild.TAG_FOR_CHILD_PROTECTION_TRUE;
		}

		m_rewardAd = null;
		m_interstitialAd = null;
		
		HwAds.RequestOptions = HwAds.RequestOptions.ToBuilder().SetNonPersonalizedAd(personalizedAd).SetTagForChildProtection(tagForChildProtection).Build();
	}

	public void RequestInterstitial()
	{
		m_interstitialAd = new InterstitialAd
		{
			AdId = HuaweiAdsIds.InterstitialId,
			AdListener = this
		};
		m_interstitialAd.LoadAd(new AdParam.Builder().Build());
	}

	public void ShowInterstitial()
	{
		m_interstitialAd.Show();
	}

	public bool NeedRequestInterstitial()
	{
		return m_interstitialAd == null;
	}

	public bool IsInterstitialAvailable()
	{
		return m_interstitialAd != null && m_interstitialAd.Loaded;
	}

	public void RequestVideo()
	{
		m_rewardAd = new RewardAd(HuaweiAdsIds.VideoId);
		m_rewardAd.LoadAd(new AdParam.Builder().Build(), OnRewardVideoLoaded, OnRewardVideoFailed);
	}

	public void ShowVideo()
	{
		m_rewardedByVideo = false;
		m_rewardAd.Show(this);
	}

	public bool NeedRequestVideo()
	{
		return m_rewardAd == null;
	}

	public bool IsVideoAvailable()
	{
		return m_rewardAd != null && m_rewardAd.Loaded;
	}

	#region Reward Video Handlers

	private void OnRewardVideoLoaded()
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			Debug.Log("HuaweiAdService -> OnRewardVideoLoaded!");
		});
	}

	private void OnRewardVideoFailed(int errorCode)
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			m_rewardAd = null;
			
			Debug.Log($"HuaweiAdService -> OnRewardVideoFailed -> ErrorCode {errorCode}");
		});
	}

	public void OnRewarded(Reward reward)
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			Debug.Log("HuaweiAdService -> OnRewarded");
			m_rewardedByVideo = true;
		});
	}

	public void OnRewardAdClosed()
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			m_rewardAd = null;
			onVideoRewarded?.Invoke(m_rewardedByVideo);
			
			Debug.Log("HuaweiAdService -> OnRewardedAdClosed");
		});
	}

	public void OnRewardAdFailedToShow(int errorCode)
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			m_rewardAd = null;
			
			Debug.Log($"HuaweiAdService -> OnRewardAdFailedToShow -> ErrorCode {errorCode}");
		});
	}

	public void OnRewardAdOpened()
	{
		
	}

	#endregion

	#region Interstitial Handlers
	
	public void OnAdClosed()
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			m_interstitialAd = null;
			onInterstitial?.Invoke(true);
			
			Debug.Log("HuaweiAdService -> OnInterstitialAdClosed");
		});
	}

	public void OnAdFailed(int reason)
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			m_interstitialAd = null;
			onInterstitial?.Invoke(false);
			
			Debug.Log($"HuaweiAdService -> OnInterstitialAdFailed -> Reason {reason}");
		});
	}

	public void OnAdLeave()
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			m_interstitialAd = null;
			onInterstitial?.Invoke(false);
			
			Debug.Log("HuaweiAdService -> OnInterstitialAdLeave");
		});
	}

	public void OnAdOpened()
	{
		
	}

	public void OnAdLoaded()
	{
		TimeController.instance.CallFromMainThread(() =>
		{
			Debug.Log("HuaweiAdService -> OnInterstitialAdLoaded!");
		});
	}

	public void OnAdClicked()
	{
		
	}

	public void OnAdImpression()
	{
		
	}

	#endregion
}