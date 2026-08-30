using UnityEngine;
using UnityEngine.UI;

public class UIPauseManager : MonoBehaviour
{

    public GameObject PausePantalla;
    public Button ResumeButton;
    public Button OpcionesButton;
    public GameObject OpcionesPantalla;
    public Button regresarButton;

    public bool IsPaused = false;
    private bool isOpcionesOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        ResumeButton.onClick.AddListener(Resume);
        OpcionesButton.onClick.AddListener(OpenOpciones);
        regresarButton.onClick.AddListener(Regresar);

    }

    public void Pause()
    {   
        if (isOpcionesOpen)
        {
            return;
        }
        PausePantalla.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsPaused = true;
    }

    public void Resume()
    {
        PausePantalla.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsPaused = false;
    }

    public void OpenOpciones()
    {   
        PausePantalla.SetActive(false);
        OpcionesPantalla.SetActive(true);
        isOpcionesOpen = true;
    }

    public void Regresar()
    {   
        isOpcionesOpen = false;
        OpcionesPantalla.SetActive(false);
        PausePantalla.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
