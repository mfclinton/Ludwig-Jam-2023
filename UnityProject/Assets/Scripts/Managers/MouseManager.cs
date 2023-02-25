using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class MouseManager : MonoBehaviour
{
    [SerializeField] GameObject mousePrefab;
    [SerializeField] float radiusScalar;

    RectTransform mouse;
    Animator animator;
    KeyboardManager keyboardManager;

    private void Awake()
    {
        // Cursor.visible = false;
        
        Canvas canvas = FindObjectOfType<Canvas>();
        mouse = Instantiate(mousePrefab, canvas.transform).GetComponent<RectTransform>();
        animator = mouse.GetComponent<Animator>();

        keyboardManager = FindObjectOfType<KeyboardManager>();
    }

    private void Update()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        // Radius
        KeyboardManager.radius = mouse.rect.width * canvas.scaleFactor * radiusScalar / 2f;
        mouse.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            keyboardManager.HandleClick();
            animator.SetTrigger("click");
        }
    }
}
