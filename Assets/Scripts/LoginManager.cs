using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public void OnStartButton()
    {
        SceneManager.LoadScene("LoginMenu");
    }

    public void OnQuitButton()
    {
        UnityEngine.Application.Quit(); // ← fully qualified, no ambiguity
    }
}