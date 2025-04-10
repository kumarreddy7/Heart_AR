using UnityEngine;

public class HeartManipulator : MonoBehaviour
{
    private float rotationSpeed = 5f;

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
                RotateHeart(touch);
        }
    }

    void RotateHeart(Touch touch)
    {
        float rotationAmount = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, -rotationAmount);
    }
}
