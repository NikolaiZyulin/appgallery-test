using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonHandler : MonoBehaviour
{
    public void BackButtonClickHandler()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
