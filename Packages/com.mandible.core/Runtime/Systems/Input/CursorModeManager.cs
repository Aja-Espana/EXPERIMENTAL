using UnityEngine;

public class CursorModeManager
{
    public enum CursorMode { Gameplay, UI }

    [Header("Cursor")]
    [SerializeField] CursorMode cursorMode;

    //Cursor
    void Start()
    {
        SetMode(cursorMode);
    }

    public void SetMode(CursorMode mode)
    {
        switch (mode)
        {
            case CursorMode.Gameplay:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case CursorMode.UI:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }
}
