using UnityEngine;
using UnityEngine.SceneManagement;

public class PausaScript : MonoBehaviour
{
    public void VolverAlMenu()
    {
        SceneManager.LoadScene("MenuEscena");
    }
}
