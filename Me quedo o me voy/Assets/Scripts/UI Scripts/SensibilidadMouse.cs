using UnityEngine;
using UnityEngine.UI;

public class SensibilidadMouse : MonoBehaviour
{
    public Slider sliderSensibilidad;
    public controlar scriptJugador; // Arrastra a tu jugador aquí en el inspector

    void Start()
    {
        // Carga el valor guardado o usa 1.0 por defecto
        float sensibilidadGuardada = PlayerPrefs.GetFloat("SensibilidadMouse", 1.0f);
        
        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.value = sensibilidadGuardada;
            // Escucha los cambios del slider
            sliderSensibilidad.onValueChanged.AddListener(CambiarSensibilidad);
        }

        AplicarSensibilidad(sensibilidadGuardada);
    }

    public void CambiarSensibilidad(float valor)
    {
        PlayerPrefs.SetFloat("SensibilidadMouse", valor);
        AplicarSensibilidad(valor);
    }

    private void AplicarSensibilidad(float valor)
    {
        // Si no asignaste el jugador en el inspector, intenta buscarlo
        if (scriptJugador == null)
        {
            scriptJugador = FindAnyObjectByType<controlar>();
        }

        if (scriptJugador != null)
        {
            scriptJugador.MouseSensitivity = valor;
        }
    }
}