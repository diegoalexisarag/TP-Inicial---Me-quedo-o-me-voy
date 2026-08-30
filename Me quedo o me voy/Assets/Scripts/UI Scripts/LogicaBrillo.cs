using UnityEngine;
using UnityEngine.UI;

public class LogicaBrillo : MonoBehaviour
{

    public Slider slider;
    public float sliderBrillo;
    public Image panelBrillo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("Brillo", 0.5f);

        panelBrillo.color = new Color(panelBrillo.color.r, panelBrillo.color.g, panelBrillo.color.b, slider.value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CambiarBrillo(float valor)
    {
        sliderBrillo = valor;
        panelBrillo.color = new Color(panelBrillo.color.r, panelBrillo.color.g, panelBrillo.color.b, slider.value);
        PlayerPrefs.SetFloat("Brillo", slider.value);
    }   
}
