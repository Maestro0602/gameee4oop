using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Source ----------")]
    [SerializeField] AudioSource musicSource;
    public AudioSource SFXSource;
    public AudioSource runningSFXSource; 

    [Header("---------- Audio Clip ----------")]
    public AudioClip background;
    public AudioClip running; 
    public AudioClip jumping;
    public AudioClip fighting;
    public AudioClip dashing;
    public AudioClip takedemage;

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
