using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class InventorySystem : MonoBehaviour
{
    // Danh sách vật phẩm trong inventory với số lượng (tên và số lượng)
    private Dictionary<string, int> items = new Dictionary<string, int>();

    // UI Elements
    public GameObject inventoryPanel; // Panel chứa toàn bộ giao diện inventory
    public TextMeshProUGUI inventoryText;        // Hiển thị danh sách vật phẩm
    public Button dropButton;         // Nút thả vật phẩm
    public Button useButton;          // Nút sử dụng vật phẩm
    public TMP_InputField itemNameInput;  // Ô nhập tên vật phẩm để thả hoặc dùng

    // Tham chiếu đến người chơi
    private GameObject player;

    // Trạng thái inventory (mở/đóng)
    private bool isInventoryOpen = false;

    void Start()
    {
        // Ẩn panel inventory ban đầu
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        // Gán sự kiện cho các nút
        dropButton.onClick.AddListener(DropItem);
        useButton.onClick.AddListener(UseItem);

        // Thêm sự kiện cho InputField
        if (itemNameInput != null)
        {
            itemNameInput.onValueChanged.AddListener(OnInputValueChanged);
            itemNameInput.interactable = false; // Tắt tương tác khi inventory đóng
        }

        // Tìm người chơi trong cảnh
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Không tìm thấy Player với tag 'Player'!");
        }

        // Cập nhật giao diện ban đầu
        UpdateInventoryUI();
    }

    void Update()
    {
        // Phát hiện phím tắt (phím "I" để mở/đóng inventory)
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    // Mở/Đóng inventory và kích hoạt InputField
    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }
        if (itemNameInput != null)
        {
            itemNameInput.interactable = isInventoryOpen; // Bật/tắt tương tác
            if (isInventoryOpen)
            {
                itemNameInput.ActivateInputField(); // Tự động focus khi mở
                UpdateInventoryUI(); // Cập nhật giao diện khi mở
            }
            else
            {
                itemNameInput.text = ""; // Xóa nội dung khi đóng
            }
        }
    }

    // Nhặt item khi va chạm (3D) và stack nếu cùng tên
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            string itemName = other.name;
            if (items.ContainsKey(itemName))
            {
                items[itemName]++;
            }
            else
            {
                items[itemName] = 1;
            }
            Destroy(other.gameObject);
            UpdateInventoryUI();
            Debug.Log($"Nhặt được: {itemName} (x{items[itemName]})");
            if (isInventoryOpen)
            {
                itemNameInput.ActivateInputField(); // Focus lại InputField khi nhặt
            }
        }
    }

    // Phản hồi khi nhập vào InputField
    void OnInputValueChanged(string input)
    {
        string trimmedInput = input.Trim().ToLower();
        if (!string.IsNullOrEmpty(trimmedInput))
        {
            foreach (var item in items.Keys)
            {
                if (item.ToLower().Contains(trimmedInput))
                {
                    Debug.Log($"Gợi ý: {item} (x{items[item]})");
                    break;
                }
            }
        }
    }

    // Thả vật phẩm khỏi inventory
    void DropItem()
    {
        if (itemNameInput != null)
        {
            string itemName = itemNameInput.text.Trim();
            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogWarning("Vui lòng nhập tên vật phẩm!");
                return;
            }
            if (items.ContainsKey(itemName) && items[itemName] > 0)
            {
                items[itemName]--;
                if (items[itemName] == 0)
                {
                    items.Remove(itemName);
                }
                SpawnDroppedItem(itemName);
                itemNameInput.text = "";
                UpdateInventoryUI();
            }
            else
            {
                Debug.LogWarning("Vật phẩm không tồn tại hoặc số lượng không đủ!");
            }
        }
    }

    // Sử dụng vật phẩm (giả lập, giảm số lượng)
    void UseItem()
    {
        if (itemNameInput != null)
        {
            string itemName = itemNameInput.text.Trim();
            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogWarning("Vui lòng nhập tên vật phẩm!");
                return;
            }
            if (items.ContainsKey(itemName) && items[itemName] > 0)
            {
                Debug.Log($"Đã sử dụng: {itemName} (còn {items[itemName] - 1})");
                items[itemName]--;
                if (items[itemName] == 0)
                {
                    items.Remove(itemName);
                }
                itemNameInput.text = "";
                UpdateInventoryUI();
            }
            else
            {
                Debug.LogWarning("Vật phẩm không tồn tại hoặc số lượng không đủ!");
            }
        }
    }

    // Tạo vật phẩm thả ra cảnh (3D)
    void SpawnDroppedItem(string itemName)
    {
        if (player != null)
        {
            Vector3 dropPosition = player.transform.position + player.transform.forward * 1.5f;
            GameObject droppedItem = new GameObject(itemName);
            droppedItem.AddComponent<MeshRenderer>();
            droppedItem.AddComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().sharedMesh;
            droppedItem.AddComponent<BoxCollider>().isTrigger = true;
            droppedItem.tag = "Item";
            droppedItem.transform.position = dropPosition;
            droppedItem.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    // Cập nhật giao diện hiển thị inventory
    void UpdateInventoryUI()
    {
        if (inventoryText != null)
        {
            inventoryText.text = "Inventory:\n" + string.Join("\n", items.Select(kvp => $"{kvp.Key} (x{kvp.Value})"));
        }
    }

    // Phương thức công khai để kiểm tra số lượng vật phẩm
    public int GetItemCount(string itemName)
    {
        return items.ContainsKey(itemName) ? items[itemName] : 0;
    }

    // Phương thức công khai để giảm số lượng vật phẩm
    public void ReduceItemCount(string itemName, int amount)
    {
        if (items.ContainsKey(itemName) && items[itemName] >= amount)
        {
            items[itemName] -= amount;
            Debug.Log($"Giảm {amount} {itemName}, còn lại: {items[itemName]}");
            if (items[itemName] <= 0)
            {
                items.Remove(itemName);
                Debug.Log($"{itemName} đã bị xóa khỏi inventory.");
            }
            UpdateInventoryUI();
        }
        else
        {
            Debug.LogWarning($"Không đủ {itemName} để giảm {amount} đơn vị.");
        }
    }
}