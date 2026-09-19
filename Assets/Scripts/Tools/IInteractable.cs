using UnityEngine;

public interface IInteractable
{
    Collider2D SelectableCollider { get; }
    void Interact();
}
