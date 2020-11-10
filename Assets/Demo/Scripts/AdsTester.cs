using UnityEngine;

public class AdsTester : MonoBehaviour
{
	[SerializeField] private GameObject m_requestInterstitialButton;
	[SerializeField] private GameObject m_requestVideoButton;
	
	[SerializeField] private GameObject m_showInterstitialButton;
	[SerializeField] private GameObject m_showVideoButton;

	private IAdService m_adService;

	private void Awake()
	{
		m_adService = new HuaweiAdService();
	}

	private void Start()
	{
		m_adService.Init();
	}

	public void RequestInterstitial()
	{
		m_adService.RequestInterstitial();
	}
	
	public void RequestRewarded()
	{
		m_adService.RequestVideo();
	}

	private void Update()
	{
		m_requestInterstitialButton.SetActive(!m_adService.IsInterstitialAvailable());
		m_showInterstitialButton.SetActive(m_adService.IsInterstitialAvailable());
		
		m_requestVideoButton.SetActive(!m_adService.IsVideoAvailable());
		m_showVideoButton.SetActive(m_adService.IsVideoAvailable());
	}

	public void ShowInterstitial()
	{
		m_adService.ShowInterstitial();
	}
	
	public void ShowRewarded()
	{
		m_adService.ShowVideo();
	}
}
