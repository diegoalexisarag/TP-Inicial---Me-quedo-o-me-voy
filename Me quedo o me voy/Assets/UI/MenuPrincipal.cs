using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Salir()
    {
        Application.Quit();
    }
    public void CrearPartida()
    {
        Debug.Log("HOST");
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene("EscenarioInicial");
    }
    public void UnirsePartida()
    {
        Debug.Log("UNIRSE");
        NetworkManager.Singleton.StartClient();
    }
}
