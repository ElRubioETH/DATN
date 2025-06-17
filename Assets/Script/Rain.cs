using UnityEngine;
using System.Collections;

public class RainController : MonoBehaviour
{
    public GameObject rainEffect; // Particle System mưa
    public float interval = 300f; // Thời gian giữa các cơn mưa (5 phút)
    public float rainDuration = 60f; // Thời gian mưa kéo dài (1 phút)

    void Start()
    {
        StartCoroutine(WeatherLoop());
    }

    IEnumerator WeatherLoop()
    {
        while (true)
        {
            // Tắt mưa, chờ 5 phút
            rainEffect.SetActive(false);
            Debug.Log("🌤️ Trời nắng...");
            yield return new WaitForSeconds(interval);

            // Bật mưa
            rainEffect.SetActive(true);
            Debug.Log("🌧️ Trời bắt đầu mưa...");
            yield return new WaitForSeconds(rainDuration);

            // Tắt mưa, lặp lại
            rainEffect.SetActive(false);
            Debug.Log("🌤️ Mưa đã tạnh.");
        }
    }
}
