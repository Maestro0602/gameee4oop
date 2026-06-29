using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ThroneRoomSequence : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image character1; // Warrior
    public Image character2; // King
    public Image fadeImage;  // Black Image (Set Alpha to 255 in Inspector)
    public GameObject theEndGraphic; // Your beautiful banner (Keep this disabled in Inspector)

    [Header("Settings")]
    public float typingSpeed = 0.09f;
    public float fadeDuration = 1.5f;
    public string menuSceneName = "LoginMenu"; // Type your actual starting scene name here

    private string[] lines = {
        "It is over, Your Majesty. The dark magic has faded from the tower... and your daughter is gone.",
        "I... I saw the shadows tearing through the stone from my chambers. It was her all along, wasn't it? The sorceress...",
        "She orchestrated everything. My banishment, the rumors, her own 'disappearance.' She used us both to steal the leylines.",
        "Forgive me, commander. I let fear cloud my vision, and I cast away my finest protector based on her whispered lies.",
        "Your apology won't mend the years I spent exiled in the wild, Your Majesty.",
        "I know. Words are hollow after such betrayal, but I intend to make it right! name your terms, your honor, your titles; they are yours",
        "I don't want my titles back. When you cast me out into the borderlands, I wasn't alone.. the beasts, the wild creatures... they sheltered me",
        "The animals? They have been barred from our stone walls for generations because hey are seen as a threat to the realm.",
        "They are the heartbeat of this land, and they protected your kingdom's true protector. My only wish is to tear down the gates.",
        "Let the wild return to the kingdom. Let the animals graze in our courtyards. We must heal what was broken.",
        "A kingdom divided from its own nature cannot stand. It is time we finally become whole again.",
        "So be it. Open the grand gates! Let the wild return... and let a new era of peace begin."
    };

    private string[] speakers = {
        "Warrior", "King", "Warrior", "King", "Warrior", "King", "Warrior", "King",  "Warrior", "King", "Warrior", "King"
    };

    private int currentLine = 0;
    private bool isTyping = false;
    private bool isTransitioning = true; // Starts true to block clicking during the initial fade-in

    void Start()
    {
        // Hide text and characters while the screen is still black
        dialogueText.text = "";
        nameText.text = "";
        character1.gameObject.SetActive(false);
        character2.gameObject.SetActive(false);

        StartCoroutine(FadeInScene());
    }

    void Update()
    {
        if (isTransitioning) return; // Block clicking while fading

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
                    StartCoroutine(FadeToTheEnd()); // Triggers the finale
                }
            }
        }
    }

    IEnumerator FadeInScene()
    {
        float elapsedTime = 0f;
        Color imageColor = fadeImage.color;

        // Smoothly clear away the black screen
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            imageColor.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            fadeImage.color = imageColor;
            yield return null;
        }

        isTransitioning = false;
        ShowLine(currentLine); // Start the dialogue
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

    IEnumerator FadeToTheEnd()
    {
        isTransitioning = true;
        dialogueText.text = "";
        nameText.text = "";

        // 1. Fade the screen to solid black
        float elapsedTime = 0f;
        Color imageColor = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            imageColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = imageColor;
            yield return null;
        }

        imageColor.a = 1f;
        fadeImage.color = imageColor;

        // 2. Turn on your "The End" banner and fade it in
        if (theEndGraphic != null)
        {
            theEndGraphic.SetActive(true);
            
            CanvasGroup canvasGroup = theEndGraphic.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = theEndGraphic.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;

            float graphicElapsedTime = 0f;
            while (graphicElapsedTime < fadeDuration)
            {
                graphicElapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(graphicElapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // 3. Wait for 4 seconds so the player can appreciate the art
        yield return new WaitForSeconds(4f);

        // 4. Return to the Main Menu
        SceneManager.LoadScene(menuSceneName);
    }
}