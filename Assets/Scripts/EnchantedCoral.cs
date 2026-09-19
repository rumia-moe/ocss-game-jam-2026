using UnityEngine;

public class EnchantedCoral : MonoBehaviour, IInteractable
{

    public Collider2D selectableCollider;
    public Collider2D SelectableCollider => selectableCollider;

    public GameObject canvas;

    public GameObject indicatorContainer;

    public GameObject Indicator => indicatorContainer;
    
    public void Interact() {
        
    }

    public void OnSelect()
    {
        canvas.SetActive(true);
    }

    public void OnUnselect()
    {
        canvas.SetActive(false);
    }

}
