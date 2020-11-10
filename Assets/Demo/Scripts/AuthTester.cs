using AuthServices;
using UnityEngine;
using UnityEngine.UI;

public class AuthTester : MonoBehaviour
{
    [SerializeField] private Text m_nickname;
    [SerializeField] private GameObject m_loginButton;
    [SerializeField] private GameObject m_logoutButton;

    private IAuthService m_authService;

    private void Awake()
    {
        m_authService = new AppGalleryService();
    }

    private void Start()
    {
        SetNickname(string.Empty);
    }

    public void Login()
    {
        m_authService.Authenticate((result) =>
        {
            if (result.success)
            {
                SetNickname(result.userName);
            }
        });
    }

    public void Logout()
    {
        m_authService.LogOut((success) =>
        {
            if (success)
            {
                SetNickname(string.Empty);
            }
        });
    }

    private void SetNickname(string nickname)
    {
        m_nickname.text = nickname;
        m_loginButton.SetActive(string.IsNullOrEmpty(nickname));
        m_logoutButton.SetActive(!string.IsNullOrEmpty(nickname));
    }

    private void OnDestroy()
    {
        Logout();
    }
}
