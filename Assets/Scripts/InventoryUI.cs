using UnityEngine;
using TMPro; // Required for TextMeshPro

public class InventoryUI : MonoBehaviour
{
    [Tooltip("Drag your TextMeshPro text object here!")]
    public TextMeshProUGUI potionText;

    void Update()
    {
        // Continuously update the text to show the current potion count
        if (InventoryManager.instance != null && potionText != null)
        {
            int currentPotions = InventoryManager.instance.GetItemCount("Health Potion");
            potionText.text = "Potions: " + currentPotions;
        }
    }
}
