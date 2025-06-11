using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun Settings")]
    public Light sun;
    public float dayDuration = 60f; // 1 vòng ngày-đêm mất bao lâu (tính bằng giây)

    [Header("Lighting Settings")]
    public Gradient sunColor; // màu mặt trời theo thời gian
    public AnimationCurve sunIntensity; // độ sáng theo thời gian

    [Header("Skybox Settings (Optional)")]
    public Material daySkybox;
    public Material nightSkybox;
    [Range(0, 1)] public float nightStart = 0.75f;
    [Range(0, 1)] public float dayStart = 0.25f;

    [Header("Fog Settings")]
    [Range(0f, 1f)] public float maxFogDensity = 0.25f;
    public AnimationCurve fogCurve = AnimationCurve.EaseInOut(0, 1, 0.5f, 0); // Tạo đường cong sáng-tối

    private float timeOfDay = 0f;

    void Update()
    {
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay > 1f) timeOfDay = 0f;

        UpdateSun();
        UpdateSkybox();
        UpdateFog();
    }

    void UpdateSun()
    {
        float angle = timeOfDay * 360f - 90f; // xoay mặt trời quanh trục
        sun.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
        sun.color = sunColor.Evaluate(timeOfDay);
        sun.intensity = sunIntensity.Evaluate(timeOfDay);
    }

    void UpdateSkybox()
    {
        if (RenderSettings.skybox == null || daySkybox == null || nightSkybox == null) return;

        if (timeOfDay >= nightStart || timeOfDay <= dayStart)
        {
            RenderSettings.skybox = nightSkybox;
        }
        else
        {
            RenderSettings.skybox = daySkybox;
        }
    }

    void UpdateFog()
    {
        float fogValue = fogCurve.Evaluate(timeOfDay); // 0 -> 1 -> 0
        RenderSettings.fogDensity = fogValue * maxFogDensity;
    }
}
