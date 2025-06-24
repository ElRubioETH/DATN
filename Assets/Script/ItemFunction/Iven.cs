using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Inventory : MonoBehaviour
{
    public GameObject inventoryPanel;
    public TMP_Text inventoryText;

    private Dictionary<string, int> items = new Dictionary<string, int>();
    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);
            if (isOpen) UpdateInventoryText();
        }
    }

    public void AddItem(string itemName)
    {
        if (items.ContainsKey(itemName))
            items[itemName]++;
        else
            items[itemName] = 1;

        if (isOpen)
            UpdateInventoryText();
    }

    public void RemoveItem(string itemName, int amount)
    {
        if (!items.ContainsKey(itemName)) return;

        items[itemName] -= amount;
        if (items[itemName] <= 0)
            items.Remove(itemName);

        if (isOpen)
            UpdateInventoryText();
    }

    public int GetItemCount(string itemName)
    {
        return items.ContainsKey(itemName) ? items[itemName] : 0;
    }

    void UpdateInventoryText()
    {
        inventoryText.text = "";
        foreach (var kvp in items)
        {
            inventoryText.text += kvp.Key + " x" + kvp.Value + "\n";
        }
    }
}
