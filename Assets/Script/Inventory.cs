using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InventorySystem : MonoBehaviour
{
    // Cấu trúc dữ liệu vật phẩm
    [System.Serializable]
    public class InventoryItem
    {
        public string itemName; // Tên vật phẩm
        public int quantity; // Số lượng
        public Sprite icon; // Hình ảnh 2D (không dùng trong trường hợp này)
        public Mesh mesh; // Mesh 3D cho vật phẩm
        public Material material; // Material cho vật phẩm (không dùng để hiển thị)
        public GameObject prefab; // Prefab 3D của vật phẩm
    }

    // Danh sách inventory và toolbar
    [SerializeField] private List<InventoryItem> inventory = new List<InventoryItem>(); // Inventory chính
    [SerializeField] private InventoryItem[] toolbar = new InventoryItem[9]; // Thanh công cụ (9 khe)
    private int selectedToolbarSlot = 0; // Khe toolbar đang chọn (0-8)

    // Thành phần UI
    public GameObject inventoryPanel; // Panel chứa inventory
    public GameObject toolbarPanel; // Panel chứa toolbar
    public GameObject slotPrefab; // Prefab cho khe inventory/toolbar
    public TextMeshProUGUI selectedItemText; // Hiển thị vật phẩm toolbar đang chọn
    private List<Image> inventorySlots = new List<Image>(); // Các khe UI của inventory
    private List<Image> toolbarSlots = new List<Image>(); // Các khe UI của toolbar
    private Image draggedItemImage; // Hình ảnh cho kéo thả
    private InventoryItem draggedItem; // Vật phẩm đang kéo
    private int draggedSlotIndex = -1; // Chỉ số khe đang kéo (-1 nếu không kéo)
    private bool isDraggingFromToolbar = false; // Cờ xác định kéo từ toolbar

    // Cấu hình
    [SerializeField] private int inventorySize = 27; // 3 hàng x 9 cột
    [SerializeField] private Vector2 slotSize = new Vector2(60, 60); // Kích thước khe
    [SerializeField] private float slotSpacing = 5f; // Khoảng cách giữa các khe

    // Tham chiếu đến người chơi
    private GameObject player;
    private bool isInventoryOpen = false;

    void Start()
    {
        // Kiểm tra các thành phần cần thiết
        if (slotPrefab == null)
        {
            Debug.LogError("slotPrefab chưa được gán trong Inspector!");
            return;
        }
        if (inventoryPanel == null)
        {
            Debug.LogError("inventoryPanel chưa được gán trong Inspector!");
            return;
        }
        if (toolbarPanel == null)
        {
            Debug.LogError("toolbarPanel chưa được gán trong Inspector!");
            return;
        }

        // Khởi tạo inventory và toolbar với kích thước rõ ràng
        inventory = new List<InventoryItem>(new InventoryItem[inventorySize]);
        for (int i = 0; i < inventorySize; i++) inventory[i] = null; // Đảm bảo tất cả ô là null
        for (int i = 0; i < toolbar.Length; i++) toolbar[i] = null;

        // Ẩn inventory panel, giữ toolbar hiển thị
        inventoryPanel.SetActive(false);
        toolbarPanel.SetActive(true);

        // Thiết lập CanvasScaler
        Canvas canvas = inventoryPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Thiết lập vị trí và kích thước panel
        SetupPanelLayout();

        // Tìm người chơi
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.LogWarning("Không tìm thấy Player với tag 'Player'!");

        // Thiết lập UI
        SetupInventoryUI();
        SetupToolbarUI();
        UpdateInventoryUI();
        UpdateToolbarUI();
        UpdateSelectedItemText();

        // Tạo hình ảnh kéo thả
        CreateDraggedItemImage();

        // Debug trạng thái ban đầu
        int emptyInventorySlots = inventory.Count(item => item == null);
        int emptyToolbarSlots = toolbar.Count(item => item == null);
        Debug.Log($"Inventory khởi tạo với {inventorySize} ô, số ô trống: {emptyInventorySlots}");
        Debug.Log($"Toolbar khởi tạo với {toolbar.Length} ô, số ô trống: {emptyToolbarSlots}");
    }

    void SetupPanelLayout()
    {
        // Thiết lập inventory panel
        RectTransform inventoryRect = inventoryPanel.GetComponent<RectTransform>();
        inventoryRect.anchorMin = new Vector2(0.5f, 0.5f);
        inventoryRect.anchorMax = new Vector2(0.5f, 0.5f);
        inventoryRect.pivot = new Vector2(0.5f, 0.5f);
        inventoryRect.sizeDelta = new Vector2((slotSize.x + slotSpacing) * 9 + 20, (slotSize.y + slotSpacing) * 3 + 40);

        // Thiết lập toolbar panel
        RectTransform toolbarRect = toolbarPanel.GetComponent<RectTransform>();
        toolbarRect.anchorMin = new Vector2(0.5f, 0);
        toolbarRect.anchorMax = new Vector2(0.5f, 0);
        toolbarRect.pivot = new Vector2(0.5f, 0);
        toolbarRect.anchoredPosition = new Vector2(0, 20);
        toolbarRect.sizeDelta = new Vector2((slotSize.x + slotSpacing) * 9 + 20, slotSize.y + 20);
    }

    void SetupInventoryUI()
    {
        if (inventoryPanel == null || slotPrefab == null) return;

        RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
        GridLayoutGroup grid = inventoryPanel.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = inventoryPanel.AddComponent<GridLayoutGroup>();
        grid.cellSize = slotSize;
        grid.spacing = new Vector2(slotSpacing, slotSpacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 9;
        grid.padding = new RectOffset(10, 10, 10, 10);

        // Xóa các khe cũ
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }
        inventorySlots.Clear();

        // Tạo các khe
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage == null)
            {
                Debug.LogError($"slotPrefab {slot.name} thiếu thành phần Image!");
                continue;
            }
            inventorySlots.Add(slotImage);

            // Thêm TextMeshProUGUI để hiển thị tên
            TextMeshProUGUI itemNameText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (itemNameText == null)
            {
                GameObject textObj = new GameObject("ItemNameText");
                textObj.transform.SetParent(slot.transform);
                itemNameText = textObj.AddComponent<TextMeshProUGUI>();
                itemNameText.alignment = TextAlignmentOptions.Center;
                itemNameText.fontSize = 12;
                RectTransform textRect = itemNameText.GetComponent<RectTransform>();
                textRect.sizeDelta = slotSize;
                textRect.anchoredPosition = Vector2.zero;
            }

            // Thêm TextMeshProUGUI cho số lượng
            TextMeshProUGUI quantityText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText == null || quantityText.name == "ItemNameText")
            {
                GameObject textObj = new GameObject("QuantityText");
                textObj.transform.SetParent(slot.transform);
                quantityText = textObj.AddComponent<TextMeshProUGUI>();
                quantityText.alignment = TextAlignmentOptions.BottomRight;
                quantityText.fontSize = 14;
                RectTransform textRect = quantityText.GetComponent<RectTransform>();
                textRect.sizeDelta = slotSize * 0.5f;
                textRect.anchoredPosition = new Vector2(-5, 5);
            }

            // Thêm tương tác
            int slotIndex = i; // Tạo biến cục bộ để capture trong lambda
            AddSlotInteractions(slot, slotIndex, false);
        }
    }

    void SetupToolbarUI()
    {
        if (toolbarPanel == null || slotPrefab == null) return;

        RectTransform panelRect = toolbarPanel.GetComponent<RectTransform>();
        GridLayoutGroup grid = toolbarPanel.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = toolbarPanel.AddComponent<GridLayoutGroup>();
        grid.cellSize = slotSize;
        grid.spacing = new Vector2(slotSpacing, 0);
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;
        grid.padding = new RectOffset(10, 10, 10, 10);

        // Xóa các khe cũ
        foreach (Transform child in toolbarPanel.transform)
        {
            Destroy(child.gameObject);
        }
        toolbarSlots.Clear();

        // Tạo các khe
        for (int i = 0; i < toolbar.Length; i++)
        {
            GameObject slot = Instantiate(slotPrefab, toolbarPanel.transform);
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage == null)
            {
                Debug.LogError($"slotPrefab {slot.name} thiếu thành phần Image!");
                continue;
            }
            toolbarSlots.Add(slotImage);

            // Thêm TextMeshProUGUI để hiển thị tên
            TextMeshProUGUI itemNameText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (itemNameText == null)
            {
                GameObject textObj = new GameObject("ItemNameText");
                textObj.transform.SetParent(slot.transform);
                itemNameText = textObj.AddComponent<TextMeshProUGUI>();
                itemNameText.alignment = TextAlignmentOptions.Center;
                itemNameText.fontSize = 12;
                RectTransform textRect = itemNameText.GetComponent<RectTransform>();
                textRect.sizeDelta = slotSize;
                textRect.anchoredPosition = Vector2.zero;
            }

            // Thêm TextMeshProUGUI cho số lượng
            TextMeshProUGUI quantityText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText == null || quantityText.name == "ItemNameText")
            {
                GameObject textObj = new GameObject("QuantityText");
                textObj.transform.SetParent(slot.transform);
                quantityText = textObj.AddComponent<TextMeshProUGUI>();
                quantityText.alignment = TextAlignmentOptions.BottomRight;
                quantityText.fontSize = 14;
                RectTransform textRect = quantityText.GetComponent<RectTransform>();
                textRect.sizeDelta = slotSize * 0.5f;
                textRect.anchoredPosition = new Vector2(-5, 5);
            }

            // Thêm tương tác
            int slotIndex = i; // Tạo biến cục bộ để capture trong lambda
            AddSlotInteractions(slot, slotIndex, true);
        }
    }

    void AddSlotInteractions(GameObject slot, int index, bool isToolbar)
    {
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null) trigger = slot.AddComponent<EventTrigger>();

        // Sự kiện click
        EventTrigger.Entry clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        clickEntry.callback.AddListener((data) => OnSlotClick(index, isToolbar));
        trigger.triggers.Add(clickEntry);

        // Sự kiện bắt đầu kéo
        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginDragEntry.callback.AddListener((data) => StartDrag(index, isToolbar));
        trigger.triggers.Add(beginDragEntry);

        // Sự kiện kết thúc kéo
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        endDragEntry.callback.AddListener((data) => EndDrag(index, isToolbar));
        trigger.triggers.Add(endDragEntry);

        // Thêm sự kiện thả (Drop) để xử lý chính xác hơn
        EventTrigger.Entry dropEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drop };
        dropEntry.callback.AddListener((data) => OnSlotDrop(index, isToolbar));
        trigger.triggers.Add(dropEntry);
    }

    void CreateDraggedItemImage()
    {
        GameObject dragObj = new GameObject("DraggedItem");
        dragObj.transform.SetParent(inventoryPanel.transform.parent);
        draggedItemImage = dragObj.AddComponent<Image>();
        draggedItemImage.raycastTarget = false;
        draggedItemImage.enabled = false;
        RectTransform rect = draggedItemImage.GetComponent<RectTransform>();
        rect.sizeDelta = slotSize;
    }

    void OnSlotClick(int index, bool isToolbar)
    {
        List<InventoryItem> source = isToolbar ? toolbar.ToList() : inventory;
        if (index >= 0 && index < source.Count && source[index] != null)
        {
            Debug.Log($"Đã click {source[index].itemName} (x{source[index].quantity})");
        }
    }

    void StartDrag(int index, bool isToolbar)
    {
        List<InventoryItem> source = isToolbar ? toolbar.ToList() : inventory;
        if (index < 0 || index >= source.Count || source[index] == null) return;

        draggedItem = new InventoryItem
        {
            itemName = source[index].itemName,
            quantity = source[index].quantity,
            icon = source[index].icon,
            mesh = source[index].mesh,
            material = source[index].material,
            prefab = source[index].prefab
        };
        draggedSlotIndex = index;
        isDraggingFromToolbar = isToolbar;
        draggedItemImage.enabled = false; // Tắt hình ảnh kéo thả vì dùng text
        Debug.Log($"Bắt đầu kéo {draggedItem.itemName} từ index {index}");
        // Xóa tạm thời từ nguồn để tránh trùng lặp
        if (isToolbar) toolbar[index] = null;
        else inventory[index] = null;
        UpdateInventoryUI();
        UpdateToolbarUI();
    }

    void EndDrag(int index, bool isToolbar)
    {
        if (draggedItem == null) return;

        DropItemAtIndex(index, isToolbar);
        draggedItem = null;
        draggedSlotIndex = -1;
        draggedItemImage.enabled = false;
        Debug.Log("Kéo thả kết thúc");
        UpdateInventoryUI();
        UpdateToolbarUI();
    }

    void OnSlotDrop(int index, bool isToolbar)
    {
        if (draggedItem != null)
        {
            Debug.Log($"Thả item {draggedItem.itemName} vào index {index}, isToolbar: {isToolbar}");
            if (isToolbar && index >= toolbar.Length)
            {
                index = FindEmptyToolbarSlot();
                Debug.Log($"Tìm ô trống trong toolbar, index mới: {index}");
            }
            DropItemAtIndex(index, isToolbar);
            draggedItem = null;
            draggedSlotIndex = -1;
            draggedItemImage.enabled = false;
            UpdateInventoryUI();
            UpdateToolbarUI();
        }
    }

    int FindEmptyToolbarSlot()
    {
        for (int i = 0; i < toolbar.Length; i++)
        {
            if (toolbar[i] == null) return i;
        }
        Debug.LogWarning("Toolbar đầy, không thể thả item!");
        return -1;
    }

    void DropItemAtIndex(int targetIndex, bool isTargetToolbar)
    {
        if (draggedItem == null) return;

        // Lấy danh sách nguồn và đích
        List<InventoryItem> sourceList = isDraggingFromToolbar ? toolbar.ToList() : inventory;
        List<InventoryItem> targetList = isTargetToolbar ? toolbar.ToList() : inventory;

        // Kiểm tra chỉ số nguồn hợp lệ
        if (draggedSlotIndex < 0 || draggedSlotIndex >= sourceList.Count)
        {
            Debug.LogWarning($"Chỉ số nguồn không hợp lệ: {draggedSlotIndex}, kích thước: {sourceList.Count}");
            // Khôi phục nếu không hợp lệ
            if (isDraggingFromToolbar) toolbar[draggedSlotIndex] = draggedItem;
            else inventory[draggedSlotIndex] = draggedItem;
            return;
        }

        // Điều chỉnh targetIndex nếu cần
        if (isTargetToolbar && (targetIndex < 0 || targetIndex >= toolbar.Length))
        {
            targetIndex = FindEmptyToolbarSlot();
            if (targetIndex == -1)
            {
                Debug.LogWarning("Toolbar đầy, không thể thả item!");
                // Khôi phục nếu không có ô trống
                if (isDraggingFromToolbar) toolbar[draggedSlotIndex] = draggedItem;
                else inventory[draggedSlotIndex] = draggedItem;
                return;
            }
        }
        else if (!isTargetToolbar && (targetIndex < 0 || targetIndex >= inventorySize))
        {
            Debug.LogWarning($"Chỉ số đích không hợp lệ: {targetIndex}, kích thước: {inventorySize}");
            // Khôi phục nếu không hợp lệ
            if (isDraggingFromToolbar) toolbar[draggedSlotIndex] = draggedItem;
            else inventory[draggedSlotIndex] = draggedItem;
            return;
        }

        // Di chuyển hoặc hoán đổi
        if (targetIndex >= 0 && targetIndex < targetList.Count)
        {
            Debug.Log($"Di chuyển {draggedItem.itemName} từ {draggedSlotIndex} ({(isDraggingFromToolbar ? "toolbar" : "inventory")}) " +
                     $"sang {targetIndex} ({(isTargetToolbar ? "toolbar" : "inventory")})");

            if (targetList[targetIndex] == null)
            {
                // Di chuyển vật phẩm
                if (isTargetToolbar) toolbar[targetIndex] = draggedItem;
                else inventory[targetIndex] = draggedItem;
            }
            else
            {
                // Hoán đổi vật phẩm
                InventoryItem temp = targetList[targetIndex];
                if (isTargetToolbar) toolbar[targetIndex] = draggedItem;
                else inventory[targetIndex] = draggedItem;
                if (isDraggingFromToolbar) toolbar[draggedSlotIndex] = temp;
                else inventory[draggedSlotIndex] = temp;
            }
        }
    }

    void Update()
    {
        if (draggedItemImage.enabled)
        {
            draggedItemImage.transform.position = Input.mousePosition;
        }

        // Mở/đóng inventory bằng phím 'I'
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        // Chọn khe toolbar bằng phím số (1-9)
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown((KeyCode)(KeyCode.Alpha1 + (i - 1))))
            {
                SelectToolbarSlot(i - 1);
            }
        }

        // Chọn khe toolbar bằng cuộn chuột
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0) SelectToolbarSlot((selectedToolbarSlot - 1 + 9) % 9);
        else if (scroll < 0) SelectToolbarSlot((selectedToolbarSlot + 1) % 9);

        // Sử dụng vật phẩm bằng click chuột trái
        if (Input.GetMouseButtonDown(0) && !isInventoryOpen && toolbar[selectedToolbarSlot] != null)
        {
            UseItem(selectedToolbarSlot, true);
        }

        // Thả vật phẩm bằng phím 'Q'
        if (Input.GetKeyDown(KeyCode.Q) && toolbar[selectedToolbarSlot] != null)
        {
            DropItem(selectedToolbarSlot, true);
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        UpdateInventoryUI();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            string itemName = other.name;
            Sprite itemIcon = null;
            Mesh itemMesh = null;
            Material itemMaterial = null;
            GameObject itemPrefab = other.gameObject;

            SpriteRenderer spriteRenderer = other.GetComponent<SpriteRenderer>();
            MeshFilter meshFilter = other.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = other.GetComponent<MeshRenderer>();
            if (spriteRenderer != null)
            {
                itemIcon = spriteRenderer.sprite;
                Debug.Log($"Nhận icon từ SpriteRenderer cho {itemName}: {itemIcon != null}, Sprite: {itemIcon?.name ?? "null"}");
            }
            else if (meshFilter != null && meshRenderer != null)
            {
                itemMesh = meshFilter.sharedMesh;
                itemMaterial = meshRenderer.sharedMaterial;
                Debug.Log($"Nhận Mesh và Material từ {itemName}: Mesh={itemMesh != null}, Material={itemMaterial != null}");
            }
            else
            {
                Debug.LogWarning($"Vật phẩm {itemName} không có SpriteRenderer hoặc MeshRenderer!");
            }

            AddItem(itemName, 1, itemIcon, itemMesh, itemMaterial, itemPrefab);
            if (inventory.Count(item => item != null) + toolbar.Count(item => item != null) >= inventorySize + toolbar.Length)
            {
                Debug.LogWarning("Không thêm được item do inventory và toolbar đầy, item bị hủy!");
            }
            else
            {
                Destroy(other.gameObject);
                Debug.Log($"Nhặt được: {itemName}");
            }
        }
    }

    void AddItem(string itemName, int quantity, Sprite icon, Mesh mesh, Material material, GameObject prefab)
    {
        Debug.Log($"Thêm item {itemName}, kiểm tra ô trống...");

        // Đếm số ô trống
        int emptyInventorySlots = inventory.Count(item => item == null);
        int emptyToolbarSlots = toolbar.Count(item => item == null);
        Debug.Log($"Số ô trống: Inventory ({emptyInventorySlots}), Toolbar ({emptyToolbarSlots})");

        // Thử xếp chồng trong toolbar trước
        for (int i = 0; i < toolbar.Length; i++)
        {
            if (toolbar[i] != null && toolbar[i].itemName == itemName)
            {
                toolbar[i].quantity += quantity;
                UpdateToolbarUI();
                Debug.Log($"Xếp chồng {itemName} trong toolbar tại index {i}");
                return;
            }
        }

        // Thử xếp chồng trong inventory
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null && inventory[i].itemName == itemName)
            {
                inventory[i].quantity += quantity;
                UpdateInventoryUI();
                Debug.Log($"Xếp chồng {itemName} trong inventory tại index {i}");
                return;
            }
        }

        // Thêm vào khe trống đầu tiên trong toolbar nếu có
        for (int i = 0; i < toolbar.Length; i++)
        {
            if (toolbar[i] == null)
            {
                toolbar[i] = new InventoryItem { itemName = itemName, quantity = quantity, icon = null, mesh = mesh, material = material, prefab = prefab };
                UpdateToolbarUI();
                Debug.Log($"Thêm {itemName} vào toolbar tại index {i}");
                return;
            }
        }

        // Thêm vào khe trống đầu tiên trong inventory
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = new InventoryItem { itemName = itemName, quantity = quantity, icon = null, mesh = mesh, material = material, prefab = prefab };
                UpdateInventoryUI();
                Debug.Log($"Thêm {itemName} vào inventory tại index {i}");
                return;
            }
        }

        Debug.LogWarning("Inventory và toolbar đều đầy!");
    }

    void DropItem(int index, bool isToolbar)
    {
        List<InventoryItem> source = isToolbar ? toolbar.ToList() : inventory;
        if (index >= 0 && index < source.Count && source[index] != null)
        {
            InventoryItem item = source[index];
            item.quantity--;
            if (item.quantity <= 0) source[index] = null;

            SpawnDroppedItem(item);
            if (isToolbar) UpdateToolbarUI();
            else UpdateInventoryUI();
            UpdateSelectedItemText();
        }
    }

    void UseItem(int index, bool isToolbar)
    {
        List<InventoryItem> source = isToolbar ? toolbar.ToList() : inventory;
        if (index >= 0 && index < source.Count && source[index] != null)
        {
            InventoryItem item = source[index];
            item.quantity--;
            if (item.quantity <= 0) source[index] = null;

            if (isToolbar) UpdateToolbarUI();
            else UpdateInventoryUI();
            UpdateSelectedItemText();
        }
    }

    void SpawnDroppedItem(InventoryItem item)
    {
        if (player != null && item.prefab != null)
        {
            Vector3 dropPosition = player.transform.position + player.transform.forward * 1.5f;
            GameObject droppedItem = Instantiate(item.prefab, dropPosition, Quaternion.identity);
            droppedItem.name = item.itemName;
            BoxCollider collider = droppedItem.GetComponent<BoxCollider>() ?? droppedItem.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            droppedItem.tag = "Item";
            droppedItem.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            MeshFilter meshFilter = droppedItem.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = droppedItem.GetComponent<MeshRenderer>();
            if (meshFilter == null) meshFilter = droppedItem.AddComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = droppedItem.AddComponent<MeshRenderer>();
            if (item.mesh != null) meshFilter.mesh = item.mesh;
            if (item.material != null) meshRenderer.material = item.material;
        }
    }

    void UpdateInventoryUI()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            Image slotImage = inventorySlots[i];
            TextMeshProUGUI itemNameText = slotImage.GetComponentInChildren<TextMeshProUGUI>();
            if (itemNameText == null || itemNameText.name != "ItemNameText")
            {
                GameObject textObj = new GameObject("ItemNameText");
                textObj.transform.SetParent(slotImage.transform);
                itemNameText = textObj.AddComponent<TextMeshProUGUI>();
                itemNameText.alignment = TextAlignmentOptions.Center;
                itemNameText.fontSize = 12;
                RectTransform textRect = itemNameText.GetComponent<RectTransform>();
                textRect.sizeDelta = slotSize;
                textRect.anchoredPosition = Vector2.zero;
            }

            TextMeshProUGUI quantityText = slotImage.GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText == null || quantityText.name != "QuantityText")
            {
                GameObject textObj = new GameObject("QuantityText");
                textObj.transform.SetParent(slotImage.transform);
                quantityText = textObj.AddComponent<TextMeshProUGUI>();
                quantityText.alignment = TextAlignmentOptions.BottomRight;
                quantityText.fontSize = 14;
                RectTransform textRect = quantityText.GetComponent<RectTransform>();
                textRect.sizeDelta = slotSize * 0.5f;
                textRect.anchoredPosition = new Vector2(-5, 5);
            }

            if (i < inventory.Count && inventory[i] != null)
            {
                slotImage.sprite = null;
                slotImage.color = Color.clear;
                itemNameText.text = inventory[i].itemName;
                itemNameText.enabled = true;
                quantityText.text = inventory[i].quantity.ToString();
            }
            else
            {
                slotImage.sprite = null;
                slotImage.color = new Color(1, 1, 1, 0.5f);
                itemNameText.text = "";
                quantityText.text = "";
            }
        }
    }

    void UpdateToolbarUI()
    {
        for (int i = 0; i < toolbarSlots.Count; i++)
        {
            Image slotImage = toolbarSlots[i];
            TextMeshProUGUI itemNameText = slotImage.GetComponentInChildren<TextMeshProUGUI>();
            if (itemNameText == null || itemNameText.name != "ItemNameText")
            {
                GameObject textObj = new GameObject("ItemNameText");
                textObj.transform.SetParent(slotImage.transform);
                itemNameText = textObj.AddComponent<TextMeshProUGUI>();
                itemNameText.alignment = TextAlignmentOptions.Center;
                itemNameText.fontSize = 12;
                RectTransform textRect = itemNameText.GetComponent<RectTransform>();
                textRect.sizeDelta = slotSize;
                textRect.anchoredPosition = Vector2.zero;
            }

            TextMeshProUGUI quantityText = slotImage.GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText == null || quantityText.name != "QuantityText")
            {
                GameObject textObj = new GameObject("QuantityText");
                textObj.transform.SetParent(slotImage.transform);
                quantityText = textObj.AddComponent<TextMeshProUGUI>();
                quantityText.alignment = TextAlignmentOptions.BottomRight;
                quantityText.fontSize = 14;
                RectTransform textRect = quantityText.GetComponent<RectTransform>();
                textRect.sizeDelta = slotSize * 0.5f;
                textRect.anchoredPosition = new Vector2(-5, 5);
            }

            if (i < toolbar.Length && toolbar[i] != null)
            {
                slotImage.sprite = null;
                slotImage.color = Color.clear;
                itemNameText.text = toolbar[i].itemName;
                itemNameText.enabled = true;
                quantityText.text = toolbar[i].quantity.ToString();
            }
            else
            {
                slotImage.sprite = null;
                slotImage.color = new Color(1, 1, 1, 0.5f);
                itemNameText.text = "";
                quantityText.text = "";
            }

            // Làm nổi bật khe được chọn
            slotImage.transform.localScale = (i == selectedToolbarSlot) ? new Vector3(1.2f, 1.2f, 1) : Vector3.one;
        }
    }

    void SelectToolbarSlot(int index)
    {
        if (index >= 0 && index < toolbar.Length)
        {
            selectedToolbarSlot = index;
            UpdateToolbarUI();
            UpdateSelectedItemText();
        }
    }

    void UpdateSelectedItemText()
    {
        if (selectedItemText != null)
        {
            selectedItemText.text = (selectedToolbarSlot >= 0 && selectedToolbarSlot < toolbar.Length && toolbar[selectedToolbarSlot] != null)
                ? $"Đang chọn: {toolbar[selectedToolbarSlot].itemName} (x{toolbar[selectedToolbarSlot].quantity})"
                : "Đang chọn: Không có";
        }
    }

    public int GetItemCount(string itemName)
    {
        int count = inventory.Where(item => item != null && item.itemName == itemName).Sum(item => item.quantity);
        count += toolbar.Where(item => item != null && item.itemName == itemName).Sum(item => item.quantity);
        return count;
    }

    public void ReduceItemCount(string itemName, int amount)
    {
        int remaining = amount;

        // Giảm từ toolbar trước
        for (int i = 0; i < toolbar.Length && remaining > 0; i++)
        {
            if (toolbar[i] != null && toolbar[i].itemName == itemName)
            {
                int reduce = Mathf.Min(toolbar[i].quantity, remaining);
                toolbar[i].quantity -= reduce;
                remaining -= reduce;
                if (toolbar[i].quantity <= 0) toolbar[i] = null;
            }
        }

        // Giảm từ inventory
        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            if (inventory[i] != null && inventory[i].itemName == itemName)
            {
                int reduce = Mathf.Min(inventory[i].quantity, remaining);
                inventory[i].quantity -= reduce;
                remaining -= reduce;
                if (inventory[i].quantity <= 0) inventory[i] = null;
            }
        }

        UpdateInventoryUI();
        UpdateToolbarUI();
        UpdateSelectedItemText();

        if (remaining > 0)
        {
            Debug.LogWarning($"Không đủ {itemName} để giảm {amount} đơn vị.");
        }
    }
}