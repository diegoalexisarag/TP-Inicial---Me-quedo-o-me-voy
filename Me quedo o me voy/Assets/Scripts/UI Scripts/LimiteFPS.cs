using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LimiteFPS : MonoBehaviour
{
    [Header("Referencias UI")]
    public Slider fpsSlider;
    public TextMeshProUGUI fpsText; // opcional: para mostrar el valor actual

    [Header("Configuración de rango")]
    [Tooltip("FPS mínimos permitidos")]
    public int minFPS = 30;

    [Tooltip("FPS máximos permitidos (el valor máximo del slider se trata como 'Ilimitado')")]
    public int maxFPS = 240;

    [Tooltip("Si está activo, el valor máximo del slider desactiva el límite (-1)")]
    public bool allowUnlimited = true;

    private const string PREF_KEY = "FPSLimit";

    void Awake()
    {
        QualitySettings.vSyncCount = 0;

        fpsSlider.minValue = minFPS;
        fpsSlider.maxValue = maxFPS;
        fpsSlider.wholeNumbers = true;

        int savedValue = PlayerPrefs.GetInt(PREF_KEY, maxFPS);
        fpsSlider.value = savedValue;

        ApplyFPS(savedValue);

        fpsSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        int fps = Mathf.RoundToInt(value);
        ApplyFPS(fps);
        PlayerPrefs.SetInt(PREF_KEY, fps);
    }

    private void ApplyFPS(int fps)
    {
        if (allowUnlimited && fps >= maxFPS)
        {
            Application.targetFrameRate = -1; // sin límite
            if (fpsText) fpsText.text = "FPS: Ilimitado";
        }
        else
        {
            Application.targetFrameRate = fps;
            if (fpsText) fpsText.text = $"FPS: {fps}";
        }
    }

    void OnDestroy()
    {
        fpsSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}