using UnityEngine;

public class CamRotation : MonoBehaviour
{
    private Transform arCameraTransform;
    private Vector3 initialOffset;

    void Start()
    {
        GameObject arCamera = GameObject.FindWithTag("MainCamera");

        if (arCamera != null)
        {
            arCameraTransform = arCamera.transform;
            initialOffset = transform.position - arCameraTransform.position;
        }
        else
        {
            Debug.LogError("AR Camera not found");
        }
    }

    void LateUpdate()
    {
        if (arCameraTransform != null)
        {
            transform.position = arCameraTransform.position + initialOffset;

            transform.rotation = Quaternion.Euler(0, arCameraTransform.eulerAngles.y, 0);
        }
    }
}
