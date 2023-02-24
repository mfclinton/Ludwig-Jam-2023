using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class KeyboardManager : MonoBehaviour
{
    KeyboardArea[] keyboardAreas;
    public static float radius = 100f;
    public static float temperature = 0.2f;
    public static Color keyColor = Color.green;

    private void Awake()
    {
        keyboardAreas = FindObjectsOfType<KeyboardArea>();
    }

    KeyboardArea GetActiveKeyboardArea()
    {
        return keyboardAreas.FirstOrDefault((ka) => ka.isFocused);
    }

    public void HandleClick()
    {
        KeyboardArea activeKeyboard = GetActiveKeyboardArea();
        if (activeKeyboard == null)
            return;

        List<(float, int)> probs = activeKeyboard.GetButtonProbabilities();
        (float p, int i) = MathUtils.Probabilities.Sample(probs);
        activeKeyboard.buttons[i].onClick.Invoke();
    }
}
