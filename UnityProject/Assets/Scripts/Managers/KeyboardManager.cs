using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class KeyboardManager : MonoBehaviour
{
    KeyboardArea[] keyboardAreas;
    public static float radius = 100f;

    //public delegate void OnKeyboardAreaUpdateHandler(string[] suggestedTweets);
    //public event OnKeyboardAreaUpdateHandler OnNewSuggestedTweets;


    private void Awake()
    {
        keyboardAreas = FindObjectsOfType<KeyboardArea>();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    KeyboardArea GetActiveKeyboardArea()
    {
        return keyboardAreas.FirstOrDefault((ka) => ka.isFocused);
    }

    void HandleClick()
    {
        KeyboardArea activeKeyboard = GetActiveKeyboardArea();
        if (activeKeyboard == null)
            return;

        List<(float, int)> probs = activeKeyboard.GetButtonProbabilities();
        (float p, int i) = MathUtils.Probabilities.Sample(probs);
        activeKeyboard.buttons[i].onClick.Invoke();
    }
}
