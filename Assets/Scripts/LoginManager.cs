using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public void OnStartButton()
    {
        SceneManager.LoadScene("OpeningDialogue");
    }

    public void OnQuitButton()
    {
        UnityEngine.Application.Quit(); 
    }
}