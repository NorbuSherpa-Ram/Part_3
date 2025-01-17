using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // The car's transform
    public Vector3 offset = new Vector3(0, 5, -10); // Offset relative to the car
    public float positionSmoothSpeed = 0.125f; // Smoothing for camera movement
    public float rotationSmoothSpeed = 5f;     // Smoothing for camera rotation

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + target.rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }
}