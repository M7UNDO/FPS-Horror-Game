using UnityEditorInternal;
using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform cameraPosition;

    private void Start()
    {
        //StartCoroutine(FetchPlay());
    }
    void Update()
    {
        transform.position = cameraPosition.position;
    }

    IEnumerator FetchPlay()
    {
        yield return new WaitForSeconds(7f);
        cameraPosition = GameObject.Find("Player").transform.GetChild(2).GetComponent<Transform>();

    }
}
