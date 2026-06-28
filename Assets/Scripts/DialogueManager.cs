using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image character1;
    public Image character2;
    public float typingSpeed = 0.09f;

    private string[] lines = {
        "It's been ages since I've been summoned to the main city, I wonder what its for?",
        "Warrior! You are aware of why you're here aren't you?",
        "No, I'm not. I'm not sure why you're calling an exiled warrior like me.",
        "Well... you see, although we've had our differences, I need your help.",
        "Oh so now you're asking for my help? What happened to 'getting rid of the threat?'",
        "Okay, I know this looks bad but I really need your help looking for Josephine!",
        "I'm not sure who Josephine is but she's not my concern, if that's all then I'm leaving.",
        "Wait! Please! She's my daughter, the princess of the city and she's been taken into the dark forest! ",
        "Please help us, you're the only one who has ever survived the forest!",
        "What's in it for me?",
        "ANYTHING! Please just get her back to us safe and sound!",
        "Then... let the animals enter the city as well, that's what I'm asking for.",
        "What?! I-I- Okay! Deal! Just keep your end and get me back my daughter please!",
        "Don't worry, I'll get her back for you, I don't make promises I can't keep."


    };

    private string[] speakers = {
        "Warrior",
        "King",
        "Warrior",
        "King",
        "Warrior",
        "King",
        "Warrior",
        "King",
        "King",
        "Warrior",
        "King",
        "Warrior",
        "King",
        "Warrior"
    };

    private int[] speakerIndex = { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 2, 1, 2, 1 };

    private int currentLine = 0;
    private bool isTyping = false;

    void Start()
    {
        ShowLine(currentLine);
    }

    void Update()
    {
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
                    ShowLine(currentLine);
                else
                    EndDialogue();
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

    void EndDialogue()
    {
        dialogueText.text = "";
        nameText.text = "";
        SceneManager.LoadScene("forest");
    }
}