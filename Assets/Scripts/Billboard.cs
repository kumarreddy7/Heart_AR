using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera arCamera;

    void Start()
    {
        arCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (arCamera != null)
        {
            transform.LookAt(transform.position + arCamera.transform.forward);
        }
    }
}
