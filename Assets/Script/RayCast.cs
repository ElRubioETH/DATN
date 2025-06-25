using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class FirstPersonInteractor : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactableLayer;
    public GameObject bluePrintPanel;
    private Camera cam;
    private bool isBluePrint = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
            {
                var controller = FindFirstObjectByType<TrainLeverController>();

                if (hit.collider.name.Contains("Start"))
                    controller.ToggleStart();
                else if (hit.collider.name.Contains("Reverse"))
                    controller.ToggleReverse();
                else if (hit.collider.name.Contains("Brake"))
                    controller.ToggleBrake();
                else if (hit.collider.name.Contains("BluePrint"))
                {
                    isBluePrint = !isBluePrint;
                    bluePrintPanel.SetActive(isBluePrint);
                }
                  
                   
            }
        }
    }
}

