using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // nếu bạn dùng TextMeshPro
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingText;

    // Gọi khi nhấn button, truyền tên scene cần load
    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(LoadSceneAsync("mapupdate"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Chờ khi đạt 100% rồi mới cho vào scene

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            loadingText.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (progress >= 1f)
            {
                loadingText.text = "100%";
                yield return new WaitForSeconds(0.5f); // delay nhẹ nếu muốn
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Ẩn panel sau khi vào scene mới
        loadingPanel.SetActive(false);
    }
}
