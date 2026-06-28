using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class InsideCastle : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image character1;
    public Image character2;
    public float typingSpeed = 0.09f;

    private string[] lines = {
        "Your Highness! Thank goodness you're unharmed, we need to move before the sorceress returns!",
        "Leave? Why would I leave? The view from this tower is spectacular like it's ready to be redrawn",
        "Princess... you’re in shock... the dark magic in this room is heavy. Do you know where the sorceress is?",
        "She gave me exactly what I came here to find... absolute isolation to hone my powers and become unstoppable...",
        "What are you talking about? The panicked letters... your father told me you were dragged away!",
        "How dense can you be? I wrote the script, my dear warrior! I knew my father would turn to the one man desperate for redemption.",
        "No... you're lying. The sorceress framed me years ago... she's the reason I was banished!",
        "Oh, she did. I did. You were far too loyal to my father. I needed you removed until my power was ready!",
        "You... you are the sorceress.",
        "There is no witch in the woods! There is only a daughter who outgrew her father's shadow!",
        "I broke back into this castle for you! I risked everything to save you!",
        "And you brought yourself right to the executioner's block, so... now... will you kneel, or do I banish you to the grave?!"
    };

    private string[] speakers = {
        "Warrior",
        "Princess",
        "Warrior",
        "Princess",
        "Warrior",
        "Princess",
        "Warrior",
        "Princess",
        "Warrior",
        "Princess",
        "Warrior",
        "Princess"
    };

    private int currentLine = 0;
    private bool isTyping = false;

    void Start() => ShowLine(currentLine);

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
        SceneManager.LoadScene("forest_boss");
    }
}