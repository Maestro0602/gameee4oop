using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossDialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image character1;
    public Image character2;
    public float typingSpeed = 0.09f;

    private string[] lines = {
        "I'm finally here... I wonder where the princess is at?",
        "Warrior! Is that you?",
        "Yes, it's me your majesty! Where are you?!",
        "Over here!"
    };

    private string[] speakers = {
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
        StopAllCoroutines();
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