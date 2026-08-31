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

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                "EscenarioInicial",
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }
    }

    public void UnirsePartida()
    {
        Debug.Log("UNIRSE");

        NetworkManager.Singleton.StartClient();
    }
}

