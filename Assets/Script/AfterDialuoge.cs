using UnityEngine;

public class AfterDialuoge : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }
    public void showpannel()
    {
        gameObject.SetActive(true);
    }
    public void hidepannel()
    {
        gameObject.SetActive(false);
    }
}
