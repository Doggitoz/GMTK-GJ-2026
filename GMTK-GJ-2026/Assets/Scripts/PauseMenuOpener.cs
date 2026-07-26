using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuOpener : MonoBehaviour
{
    [SerializeField]
    GameObject _pauseMenuObject;

    InputAction _pauseAction;
    private void Awake()
    {
        _pauseAction = InputSystem.actions.FindAction("Pause");
    }


    private void OnEnable()
    {
        _pauseAction.performed += TogglePauseMenu;
    }

    private void OnDisable()
    {
        _pauseAction.performed -= TogglePauseMenu;
    }

    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        _pauseMenuObject.SetActive(!_pauseMenuObject.activeInHierarchy);
    }
}
