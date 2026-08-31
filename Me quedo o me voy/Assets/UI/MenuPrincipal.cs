using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField codigoInput;

    public void Salir()
    {
        Application.Quit();
    }

    public void CrearPartida()
    {
        Debug.Log("CREAR PARTIDA");

        MultiplayerManager.Instance.CrearPartida();
    }

    public void UnirsePartida()
    {
        Debug.Log("BOTON UNIRSE FUNCIONA");

        string codigo = codigoInput.text;

        Debug.Log("CODIGO ESCRITO: [" + codigo + "]");

        MultiplayerManager.Instance.UnirsePartida(codigo);
    }
}