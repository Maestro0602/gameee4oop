using UnityEngine;
using UnityEngine.SceneManagement;

public class CastleTrigger : MonoBehaviour
{
    public string sceneName = "forest_boss";

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Triggered by: " + other.name + " tag: " + other.tag);
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}