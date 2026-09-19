using UnityEngine;

public interface IInteractable
{
    Collider2D SelectableCollider { get; }
    GameObject Indicator { get;  }
    void Interact();
    void OnSelect();
    void OnUnselect();
}
