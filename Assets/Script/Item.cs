using UnityEngine;


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