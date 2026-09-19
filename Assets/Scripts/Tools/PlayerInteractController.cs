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

    private ContactFilter2D filter;

    private void OnEnable()
    {
        interactables = new List<Collider2D>();
        interact = playerActionMap.FindAction("Interact");
        left = playerActionMap.FindAction("UI_LEFT");
        right = playerActionMap.FindAction("UI_RIGHT");
        up = playerActionMap.FindAction("UI_UP");
        down = playerActionMap.FindAction("UI_DOWN");

        interact.performed += OnInteract;

        left.performed += OnUIMovePositive;
        down.performed += OnUIMovePositive;

        up.performed += OnUIMoveNegative;
        right.performed += OnUIMoveNegative;

        filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Default"));
        filter.useLayerMask = true;
    }

    private void OnDisable()
    {
        interact.performed -= OnInteract;

        left.performed -= OnUIMovePositive;
        down.performed -= OnUIMovePositive;

        up.performed -= OnUIMoveNegative;
        right.performed -= OnUIMoveNegative;
    }


    private Collider2D[] hits = new Collider2D[10];

    
    public void Update()
    {
        
        int hitCount = Physics2D.OverlapCircle(transform.position, player.insight, filter, hits);
        interactables.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hits[i];
            //if(hit.TryGetComponent<IInteractable>(out var o) && hit == o.selectableCollider)
            //{
            //    interactables.Add(hit);
            //}
            if (hit.TryGetComponent<IInteractable>(out var o) && hit == o.SelectableCollider)
            {
                interactables.Add(hit);
            }
        }

        Debug.Log(interactables.Count);

        interactables.Sort((a, b) =>
        {
            Vector3 pa = a.transform.position;
            Vector3 pb = b.transform.position;
            int byX = pa.x.CompareTo(pb.x);
            return byX != 0 ? byX : pa.y.CompareTo(pb.y);
        });

        if (currentInteractable != null && interactables.Count > 0 && !(interactables.Contains(currentInteractable)))
        {
            unselectCurrent();
            currentInteractable = interactables[0];
            index = 0;
        }

        if(interactables.Count == 0)
        {
            unselectCurrent();
            currentInteractable = null;
            index = 0;
        }
        
        if(currentInteractable != null)
        {
            selectCurrent();
        }else if(currentInteractable == null && interactables.Count > 0)
        {
            currentInteractable = interactables[0];
            selectCurrent();
            index = 0;
        }

    }

    public void selectCurrent()
    {
        if (currentInteractable != null)
        {
            currentInteractable.GetComponent<SpriteRenderer>().color = Color.black;
        }
    }

    public void unselectCurrent()
    {
        if (currentInteractable != null)
        {
            currentInteractable.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void OnUIMovePositive(InputAction.CallbackContext context)
    {
        if (interactables.Count != 0)
        {
            index += 1;
            unselectCurrent();
            if (index >= interactables.Count)
            {
                index = 0;
            }
            currentInteractable = interactables[index];
            selectCurrent();
        }
        else
        {
            unselectCurrent();
        }
    }

    public void OnUIMoveNegative(InputAction.CallbackContext context)
    {
        if (interactables.Count != 0)
        {
            index -= 1;
            unselectCurrent();
            if (index < 0)
            {
                index = interactables.Count - 1;
            }
            currentInteractable = interactables[index];
            selectCurrent();
        }
        else
        {
            unselectCurrent();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (currentInteractable != null) {
            currentInteractable.GetComponent<IInteractable>().Interact();
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, player.insight);
    }

}
