using UnityEngine;

public class UICameraSwitcher : MonoBehaviour
{
    [SerializeField] private Canvas worldCanvas;

    public void SetCanvasCamera(Camera newCamera)
    {
        if (worldCanvas != null)
            worldCanvas.worldCamera = newCamera;
    }
}
