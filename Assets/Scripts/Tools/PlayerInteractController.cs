using UnityEngine;
using UnityEngine.Assemblies;
using UnityEngine.InputSystem;

public class PlayerInteractController : MonoBehaviour
{

    private IInteractable currentInteractable;

    private InputAction interact;

    private void Start()
    {
        interact = InputSystem.actions.FindActionMap("Player", true).FindAction("interact");

        interact.performed += OnInteract;
    }

    public void Update()
    {
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        currentInteractable.Interact();
    }


}
