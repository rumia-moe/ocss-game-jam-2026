using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteractController : MonoBehaviour
{

    private int index = 0;

    public InputActionAsset playerActionMap;

    private InputAction interact;

    private InputAction left;
    private InputAction right;
    private InputAction down;
    private InputAction up;

    private ContactFilter2D filter;

    private void OnEnable()
    {
        interact = playerActionMap.FindAction("Interact");
        left = playerActionMap.FindAction("UI_LEFT");
        right = playerActionMap.FindAction("UI_RIGHT");
        up = playerActionMap.FindAction("UI_UP");
        down = playerActionMap.FindAction("UI_DOWN");

        // interact.performed += OnInteract;

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
        // interact.performed -= OnInteract;

        left.performed -= OnUIMovePositive;
        down.performed -= OnUIMovePositive;

        up.performed -= OnUIMoveNegative;
        right.performed -= OnUIMoveNegative;
    }

    private Collider2D[] hits = new Collider2D[10];
    private int selectedIndex = 0;

    public void Update()
    {

        int hitCount = Physics2D.OverlapCircle(transform.position, GetComponent<Entity>().insight, filter, hits);
        selectedIndex = (index % hitCount + hitCount) % hitCount;

        for (int i = 0; i < hitCount; i++)
        {
            if (i == selectedIndex)
            {
                hits[i].GetComponent<SpriteRenderer>().color = Color.black;
            }
            else
            {
                hits[i].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

    }

    public void OnUIMovePositive(InputAction.CallbackContext context)
    {
        this.index++;
    }

    public void OnUIMoveNegative(InputAction.CallbackContext context)
    {
        this.index--;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        hits[selectedIndex].GetComponent<IInteractable>().Interact();
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, GetComponent<Entity>().insight);
    }

}
