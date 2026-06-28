using UnityEngine;
using UnityEngine.UI;

public class UIAnimator : MonoBehaviour
{
    public Sprite[] frames;      
    public float fps = 8f;      

    private Image image;
    private int currentFrame;
    private float timer;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
            image.sprite = frames[currentFrame];
        }
    }
}