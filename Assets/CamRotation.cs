using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 10f;
    public float orbitSpeed = 5f;

    private float yaw = 45f;
    private float pitch = 30f;

    void LateUpdate()
    {
        if (!target) return;

        // Clic Droit pour tourner la caméra
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * orbitSpeed;
            pitch -= Input.GetAxis("Mouse Y") * orbitSpeed;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }

        // Molette Zoom
        distance -= Input.mouseScrollDelta.y;
        distance = Mathf.Clamp(distance, 5f, 20f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 position = rotation * new Vector3(0, 0, -distance) + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }
}