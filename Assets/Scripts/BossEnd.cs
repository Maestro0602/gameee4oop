using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ArenaDefeatSequence : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image character1; // Warrior
    public Image character2; // Princess
    public Image fadeImage;  // Fullscreen black image
    public float typingSpeed = 0.09f;
    public float fadeDuration = 1.5f;

    // Change this to the exact name of your Throne Room Unity scene
    public string nextSceneName = "ThroneRoomScene";

    private string[] lines = {
        "This... this is impossible! You were cast out, broken, nothing! How do you still possess such strength?!",
        "My strength didn't come from a crown or a sterile tower. It came from surviving the wild you left me to rot in.",
        "Mark my words, you filthy creature... the shadow poetry cannot be completely erased. I will return, and this kingdom will burn!" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
        "She's... gone. The magical pressure is dissipating, but the tower is collapsing. I need to make it to the main hall."
    };

    private string[] speakers = {
        "Princess", "Warrior", "Princess", "Warrior"
    };

    private int currentLine = 0;
    private bool isTyping = false;
    private bool isTransitioning = false;

    void Start()
    {
        // Ensure fade image starts fully transparent
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        ShowLine(currentLine);
    }

    void Update()
    {
        if (isTransitioning) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = lines[currentLine];
                isTyping = false;
            }
            else
            {
                currentLine++;
                if (currentLine < lines.Length)
                {
                    ShowLine(currentLine);
                }
                else
                {
                    StartCoroutine(FadeAndLoadNextScene());
                }
            }
        }
    }

    void ShowLine(int index)
    {
        nameText.text = speakers[index];
        SetSpeaker(index);
        StartCoroutine(TypeLine(lines[index]));
    }

    void SetSpeaker(int index)
    {
        bool isWarrior = speakers[index] == "Warrior";
        character1.gameObject.SetActive(isWarrior);
        character2.gameObject.SetActive(!isWarrior);
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    IEnumerator FadeAndLoadNextScene()
    {
        isTransitioning = true;
        dialogueText.text = "";
        nameText.text = "";

        // Fade to solid black
        float elapsedTime = 0f;
        Color imageColor = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            imageColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = imageColor;
            yield return null;
        }

        // Load the entirely separate scene
        SceneManager.LoadScene(nextSceneName);
    }
}