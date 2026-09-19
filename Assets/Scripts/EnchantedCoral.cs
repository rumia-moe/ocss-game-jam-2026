using UnityEngine;

public class EnchantedCoral : MonoBehaviour, IInteractable
{

    public Collider2D selectableCollider;
    public Collider2D SelectableCollider => selectableCollider;

    public GameObject canvas;
    
    public void Interact() {
        canvas.SetActive(!canvas.activeSelf);
    }

}
