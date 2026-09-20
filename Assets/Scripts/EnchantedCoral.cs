using UnityEngine;

public class EnchantedCoral : MonoBehaviour, IInteractable
{

    public Collider2D selectableCollider;
    public Collider2D SelectableCollider => selectableCollider;

    public GameObject canvas;

    public GameObject indicatorContainer;

    public GameObject Indicator => indicatorContainer;
    
    public void Interact() {
        canvas.SetActive(!canvas.activeSelf);
    }

    public void OnSelect()
    {
        Indicator.SetActive(true);
    }

    public void OnUnselect()
    {
        Indicator.SetActive(false);
        canvas.SetActive(false);
    }

}
