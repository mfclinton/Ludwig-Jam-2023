using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public Texture2D crosshair;
    public float opacity;

    private void Awake()
    {
        SetCursor();
    }

    private void SetCursor()
    {
        //set the cursor origin to its centre. (default is upper left corner)
        Vector2 cursorOffset = new Vector2(crosshair.width / 2, crosshair.height / 2);
        Cursor.SetCursor(crosshair, cursorOffset, CursorMode.ForceSoftware);

    }
}
