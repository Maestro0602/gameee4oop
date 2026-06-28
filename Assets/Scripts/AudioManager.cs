using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("---------- Audio Source ----------")]
    [SerializeField] AudioSource musicSource;
    public AudioSource SFXSource;
    public AudioSource runningSFXSource;

    [Header("---------- Audio Clip ----------")]
    public AudioClip background;
    public AudioClip running;
    public AudioClip jumping;
    public AudioClip fighting;
    public AudioClip takingDamage;
    public AudioClip dashing;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();

        runningSFXSource.clip = running;
        runningSFXSource.loop = true;
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}