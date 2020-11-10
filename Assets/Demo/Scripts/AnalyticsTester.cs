using System.Collections.Generic;
using Analytics;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsTester : MonoBehaviour
{
	[SerializeField] private Text m_eventNameText;
	[SerializeField] private Text m_eventKeyText;
	[SerializeField] private Text m_eventValueText;
	
	[SerializeField] private Text m_userIdText;

	private IAnalytic m_analytic;

	private void Awake()
	{
		m_analytic = new HuaweiAnalytic();
	}

	private void Start()
	{
		m_analytic.Init();
	}

	public void SetUserId()
	{
		m_analytic.SetUserID(m_userIdText.text);
	}

	public void LogEvent()
	{
		m_analytic.LogEvent(m_eventNameText.text, new Dictionary<string, string>()
		{
			{m_eventKeyText.text, m_eventValueText.text}
		});
	}
}
