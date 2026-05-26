using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    void Start()
    {
        // This will only destroy the object it is attached to (the slash)
        Destroy(gameObject, 0.2f);
    }
}

