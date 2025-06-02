using UnityEngine;

public class HideFromCamera : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera; // Camera không được thấy GameObject này
    public GameObject objectToHide; // GameObject cần ẩn
    public string hiddenLayerName = "HiddenFromCamera";

    void Start()
    {
        // Tạo layer nếu chưa có
        int hiddenLayer = LayerMask.NameToLayer(hiddenLayerName);
        if (hiddenLayer == -1)
        {
            Debug.LogError($"Layer '{hiddenLayerName}' chưa được tạo. Hãy tạo nó trong Unity Editor trước.");
            return;
        }

        // Gán layer cho GameObject cần ẩn
        objectToHide.layer = hiddenLayer;

        // Loại layer khỏi Culling Mask của camera
        targetCamera.cullingMask &= ~(1 << hiddenLayer);
    }
}
