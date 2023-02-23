using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseManager : MonoBehaviour
{
    [SerializeField] Texture2D crosshair;
    [SerializeField] Vector2 offsetFactor = Vector2.one;
    [SerializeField] float radiusFactor = 1f;
    [SerializeField] Image debugCircle;
    KeyboardManager keyboardManager;

    private void Awake()
    {
        keyboardManager = FindObjectOfType<KeyboardManager>();
        SetCursor(crosshair, offsetFactor, radiusFactor);
    }

    public void Update()
    {
        // DebugCursor();
        if (Input.GetMouseButtonDown(0))
            keyboardManager.HandleClick();
    }

    private void DebugCursor()
    {
        // TODO: Fix circle scale
        debugCircle.transform.GetComponent<RectTransform>().sizeDelta = Vector2.one * (crosshair.width * radiusFactor);
        debugCircle.transform.position = Input.mousePosition;
    }

    private void SetCursor(Texture2D texture, Vector2 offsetFactor, float radiusFactor)
    {
        //set the cursor origin to its centre. (default is upper left corner)
        Vector2 cursorOffset = new Vector2(texture.width / 2, texture.height / 2) * offsetFactor;
        Cursor.SetCursor(texture, cursorOffset, CursorMode.ForceSoftware);
        KeyboardManager.radius = texture.width * radiusFactor;

    }
}
