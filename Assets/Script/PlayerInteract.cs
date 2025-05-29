using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public Camera playerCamera;

    private LeverController currentLever;

    void Update()
    {
        CheckForLever();

        if (currentLever != null && Input.GetKeyDown(KeyCode.E))
        {
            currentLever.ToggleLever();
        }
    }

    void CheckForLever()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            LeverController lever = hit.collider.GetComponent<LeverController>();

            if (lever != null)
            {
                if (lever != currentLever)
                {
                    currentLever?.ShowUI(false);
                    currentLever = lever;
                    currentLever.ShowUI(true);
                }
                return;
            }
        }

        if (currentLever != null)
        {
            currentLever.ShowUI(false);
            currentLever = null;
        }
    }
}
