using UnityEngine;

public class TrainStarter : MonoBehaviour
{
    private void Awake()
    {
        // Cách 1: Nếu TrainController nằm trên cùng GameObject
        TrainController controller = GetComponent<TrainController>();

        // Cách 2: Nếu TrainController nằm trên GameObject khác, bạn cần tham chiếu
        // [SerializeField] private TrainController controller;

        if (controller != null)
        {
            controller.StartTrain();
        }
        else
        {
            Debug.LogError("TrainController không được tìm thấy!");
        }
    }
}
