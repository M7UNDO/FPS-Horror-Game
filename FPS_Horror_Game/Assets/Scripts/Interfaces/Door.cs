using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Door opens");
        // Door-specific open logic here
    }
}
