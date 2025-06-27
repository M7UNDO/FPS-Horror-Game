using UnityEngine;

public class CharacterControllerCollisionDetector : MonoBehaviour
{
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogWarning("No CharacterController found on this GameObject.");
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Called when CharacterController hits another collider
        Debug.Log($"CharacterController hit: {hit.gameObject.name}");

        // Example: React to hitting an enemy or interactable
        // if (hit.gameObject.CompareTag("Enemy"))
        // {
        //     Debug.Log("Hit an enemy!");
        // }
    }
}
