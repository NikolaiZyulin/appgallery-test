using HuaweiMobileServices.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelector : MonoBehaviour
{
    private void Start()
    {
        if (!ServiceHelper.HMSAvailable)
        {
            Application.Quit();
        }
    }

    public void AdsClickHandler()
    {
        LoadScene("Ads");
    }
    
    public void InAppClickHandler()
    {
        LoadScene("IAP");
    }
    
    public void PushClickHandler()
    {
        LoadScene("Push");
    }
    
    public void CrashClickHandler()
    {
        LoadScene("Crash");
    }
    
    public void AnalyticsClickHandler()
    {
        LoadScene("Analytics");
    }
    
    public void AuthClickHandler()
    {
        LoadScene("Auth");
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
