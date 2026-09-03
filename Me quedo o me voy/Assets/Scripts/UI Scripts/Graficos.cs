using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

public class Graficos : MonoBehaviour
{
    public RenderPipelineAsset[] qualityLevels;
    public TMP_Dropdown dropdown;
    void Start()
    {
        dropdown.value = QualitySettings.GetQualityLevel();
    }
    public void ChangeLevel(int value)
    {
        QualitySettings.SetQualityLevel(value);
        QualitySettings.renderPipeline = qualityLevels[value];
    }
}
