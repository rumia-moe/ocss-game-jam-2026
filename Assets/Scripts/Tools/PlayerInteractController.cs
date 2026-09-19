using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.Assemblies;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteractController : MonoBehaviour
{
    private Player player;

    private List<IInteractable> interactables;

    private IInteractable currentInteractable;
    private int index;

    private InputActionMap playerActionMap;

    private InputAction interact;

    private InputAction left;
    private InputAction right;
    private InputAction down;

    private InputAction up;

    private void Start()
    {
        player = GetComponent<Player>();

        playerActionMap = InputSystem.actions.FindActionMap("Player", true);


        interact = playerActionMap.FindAction("interact");
        left = playerActionMap.FindAction("UI_LEFT");
        right = playerActionMap.FindAction("UI_RIGHT");
        up = playerActionMap.FindAction("UI_UP");
        down = playerActionMap.FindAction("UI_DOWN");


        interact.performed += OnInteract;

        left.performed += OnUIMovePositive;
        down.performed += OnUIMovePositive;

        up.performed += OnUIMoveNegative;
        right.performed += OnUIMoveNegative;
    }

    public void Update()
    {
        Collider2D[] hits = Physics2D.OverlapCircle(transform.position, player.insight);

        foreach (Collider2D hit in hits)
        {
            if(hit.TryGetComponent<IInteractable>(out var o)){
                interactables.Add(o);
            }
        }

        if (currentInteractable != null || !(interactables.Contains(currentInteractable)))
        {
            currentInteractable = interactables[0];
            index = 0;
        }

    }

    public void OnUIMovePositive(InputAction.CallbackContext context)
    {
        index += 1;
        if(index >= interactables.Count)
        {
            index = 0;
        }

        currentInteractable = interactables[index];
    }

    public void OnUIMoveNegative(InputAction.CallbackContext context)
    {
        index -= 1;
        if (index < 0)
        {
            index = interactables.Count - 1;
        }

        currentInteractable = interactables[index];
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        currentInteractable.Interact();
    }


}
