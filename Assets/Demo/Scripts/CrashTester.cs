using Services.Debug;
using UnityEngine;
using UnityEngine.UI;

public class CrashTester : MonoBehaviour
{
	[SerializeField] private GameObject m_buttons;
	
	[SerializeField] private Text m_customKeyText;
	[SerializeField] private Text m_customValueText;
    
	[SerializeField] private Text m_userIdText;

	private ICrashlyticsUtils m_crashlyticsUtils;
	private bool m_initialized;

	private void Awake()
	{
		m_crashlyticsUtils = new HuaweiCrashlyticsUtils();
	}

	private void Start()
	{
		m_crashlyticsUtils.Init(success =>
		{
			m_initialized = success;
			UpdateState();
		});
	}

	private void UpdateState()
	{
		m_buttons.SetActive(m_initialized);
	}

	public void ThrowException()
	{
		m_crashlyticsUtils.Crash();
	}

	public void SetCustomKey()
	{
		m_crashlyticsUtils.SetCustomKey(m_customKeyText.text, m_customValueText.text);
	}

	public void SetUserId()
	{
		m_crashlyticsUtils.SetUserId(m_userIdText.text);
	}
}
