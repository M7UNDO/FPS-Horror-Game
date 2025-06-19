using UnityEditorInternal;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform cameraPosition;

    // Update is called once per frame
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
