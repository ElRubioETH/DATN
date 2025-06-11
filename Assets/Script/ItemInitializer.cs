using UnityEngine;

public class ItemInitializer : MonoBehaviour
{
    public Item item; // Kéo thả Item vào đây trong Inspector

    void Start()
    {
        ItemWorld itemWorld = GetComponent<ItemWorld>();
        if (itemWorld != null && item != null)
        {
            itemWorld.SetItem(item);
            Debug.Log("ItemInitializer đã gán Item: " + item.itemName);
        }
        else
        {
            Debug.LogError("Item hoặc ItemWorld không được gán!");
        }
    }
}