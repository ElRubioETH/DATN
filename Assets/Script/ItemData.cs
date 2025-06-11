using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public int quantity;
    public ItemType itemType;
    public int value;

    public Item(string name, int qty, ItemType type, int val)
    {
        itemName = name;
        quantity = qty;
        itemType = type;
        value = val;
    }
}

public enum ItemType
{
    Weapon,
    Potion,
    Resource,
    Food,
    Other
}

