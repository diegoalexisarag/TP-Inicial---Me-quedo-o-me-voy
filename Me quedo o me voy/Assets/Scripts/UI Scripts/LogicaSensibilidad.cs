using UnityEngine;
using UnityEngine.UI;

public class LogicaSensibilidad : MonoBehaviour
{

    public Slider slider;
    public float sliderSensibilidad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CambiarSensibilidad(float valor)
    {
        sliderSensibilidad = valor;
        PlayerPrefs.SetFloat("Sensibilidad", slider.value);
        controlar controlador = FindAnyObjectByType<controlar>();
        Debug.Log("Controlador encontrado: " + (controlador != null ? "Sí" : "No"));
        if (controlador != null)
        {
            controlador.MouseSensitivity = slider.value;
        }
    }
}
