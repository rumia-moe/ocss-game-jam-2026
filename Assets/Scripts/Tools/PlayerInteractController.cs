using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.Assemblies;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteractController : MonoBehaviour
{
    public Player player;

    private List<Collider2D> interactables;

    private Collider2D currentInteractable;
    private int index;

    public InputActionAsset playerActionMap;

    private InputAction interact;

    private InputAction left;
    private InputAction right;
    private InputAction down;

    private InputAction up;

    private void Start()
    {
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, player.insight);

        interactables = new List<Collider2D>();

        foreach (Collider2D hit in hits)
        {
            if(hit.TryGetComponent<IInteractable>(out var o) && hit == hit.GetComponent<Entity>().overtnessCollider)
            {
                interactables.Add(hit);
            }
        }

        if (currentInteractable != null & interactables.Count > 0 & !(interactables.Contains(currentInteractable)))
        {
            unselect(currentInteractable);
            currentInteractable = interactables[0];
            index = 0;
        }

        if(interactables.Count == 0)
        {
            unselect(currentInteractable);
            currentInteractable = null;
            index = 0;
        }
        
        if(currentInteractable != null)
        {
            select(currentInteractable);
        }else if(currentInteractable == null & interactables.Count > 0)
        {
            currentInteractable = interactables[0];
            select(currentInteractable);
            index = 0;
        }

    }

    public void select(Collider2D collider)
    {
        if (collider != null)
        {
            currentInteractable.GetComponent<SpriteRenderer>().color = Color.black;
        }
    }

    public void unselect(Collider2D collider)
    {
        if (collider != null)
        {
            currentInteractable.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void OnUIMovePositive(InputAction.CallbackContext context)
    {
        if (interactables.Count != 0)
        {
            index += 1;
            unselect(currentInteractable);
            if (index > interactables.Count)
            {
                index = 0;
            }

            currentInteractable = interactables[index];
            select(currentInteractable);
        }
    }

    public void OnUIMoveNegative(InputAction.CallbackContext context)
    {
        if (interactables.Count != 0)
        {
            index -= 1;
            unselect(currentInteractable);
            if (index < 0)
            {
                index = interactables.Count - 1;
            }
            currentInteractable = interactables[index];
            select(currentInteractable);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        currentInteractable.GetComponent<IInteractable>().Interact();
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, player.insight);
    }

}
