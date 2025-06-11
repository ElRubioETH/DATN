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

public class ItemWorld : MonoBehaviour
{
    [SerializeField] private Item item;

    public void SetItem(Item newItem)
    {
        item = newItem;
        if (gameObject != null)
        {
            gameObject.name = item.itemName;
        }
    }

    public Item GetItem()
    {
        return item;
    }

    void Start()
    {
        if (item == null && gameObject.name != null)
        {
            item = new Item(gameObject.name, 1, ItemType.Food, 10); // Giá trị mặc định
        }
    }
}