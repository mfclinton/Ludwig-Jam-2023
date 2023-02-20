using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Button[] buttons;
    bool isFocused;
    float[] distances;

    private void Awake()
    {
        buttons = GetComponentsInChildren<Button>();
        distances = new float[buttons.Length];
    }

    private void Update()
    {
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        if (!isFocused)
            return;

        UpdateButtonDistances();
    }

    public void UpdateButtonDistances()
    {
        Vector2 mousePosition = Input.mousePosition;
        for (int i = 0; i < buttons.Length; i++)
        {
            Button b = buttons[i];
            distances[i] = Vector3.Distance(mousePosition, b.transform.position);
            b.image.color = Color.green * (1f - ((distances[i] - 50) / 50));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Cursor Entering " + name + " GameObject");
        isFocused = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Cursor Exiting " + name + " GameObject");
        isFocused = false;
    }
}
