using TMPro;
using UnityEngine;

public class MostrarCodigoSala : MonoBehaviour
{
    [SerializeField] private TMP_Text textoCodigo;

    private void Start()
    {
        if (MultiplayerManager.Instance == null)
        {
            Debug.LogError("No existe MultiplayerManager.");
            return;
        }

        textoCodigo.text = "Código de sala: " + MultiplayerManager.Instance.JoinCode;
    }
}