using UnityEngine;

public class Interactabletest : MonoBehaviour, IInteractable
{

    public string prompt;

    public void Interact()
    {
        Debug.Log(prompt);
    }
}
