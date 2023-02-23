using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MathUtils;

public class KeyboardArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button[] buttons { get; private set; }
    public bool isFocused { get; private set; }
    public float[] dists { get; private set; }

    private void Awake()
    {
        buttons = GetComponentsInChildren<Button>();
        dists = new float[buttons.Length];
    }

    private void Update()
    {
        UpdateButtons();
    }

    void UpdateButtons()
    {
        if (!isFocused)
            return;

        UpdateButtonDistances();
        UpdateButtonColors();
    }

    void UpdateButtonDistances()
    {
        Vector2 mousePosition = Input.mousePosition;
        
        for (int i = 0; i < buttons.Length; i++)
        {
            Button b = buttons[i];
            dists[i] = Vector3.Distance(mousePosition, b.transform.position);
        }
    }

    void UpdateButtonColors()
    {
        List<(float, int)> probs = GetButtonProbabilities();

        // Clear
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].image.color = Color.white;

        if(isFocused)
            foreach((float p, int i) in probs)
                buttons[i].image.color = Color.Lerp(Color.white, Color.red, p);
    }

    public List<(float, int)> GetButtonProbabilities()
    {
        List<(float, int)> values = new List<(float, int)>();

        float max_value = 0f;
        int max_index = -1;

        for (int i = 0; i < dists.Length; i++)
        {
            float v = (KeyboardManager.radius - dists[i]) / KeyboardManager.radius;
            if(max_value < v || max_index == -1)
            {
                max_index = i;
                max_value = v;
            }

            if (0f <= v)
            {
                values.Add((v, i));
            }
        }

        if (values.Count == 0)
            values.Add((max_value, max_index));

        return MathUtils.Probabilities.CalculateProbs(values);
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
        UpdateButtonColors();
    }
}
