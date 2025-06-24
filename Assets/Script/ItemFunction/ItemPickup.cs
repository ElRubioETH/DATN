using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemName = "Apple";

    void OnMouseOver()
    {
        //if (Vector3.Distance(Camera.main.transform.position, transform.position) < 3f)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Inventory playerInventory = FindFirstObjectByType<Inventory>();
                QuestManager questManager = FindFirstObjectByType<QuestManager>();


                if (playerInventory != null)
                {
                    playerInventory.AddItem(itemName);
                    questManager?.OnItemCollected(itemName);
                }

                Destroy(gameObject);
            }
        }
    }
}
