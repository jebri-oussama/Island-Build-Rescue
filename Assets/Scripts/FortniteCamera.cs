using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player; // Assign your player object in the Inspector
    public float rotationSpeed = 10.0f;
    public Vector3 offset; // Now an offset from the *player*
    public float smoothSpeed = 0.125f;
    public float verticalRotationLimit = 80f; // Limit for up/down camera movement

    private float currentYRotation = 0f; // Store the current Y rotation of the *player*
    private float currentXRotation = 0f; // Store the current X rotation of the camera

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player Transform not assigned!");
            enabled = false;
            return;
        }

        // Calculate the offset from the *player*
        offset = transform.localPosition; // Since camera is a child

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        // Rotation (Smooth and Stable)
        float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed;
        currentYRotation += horizontalInput * Time.deltaTime;

        // Rotate the PLAYER (parent)
        player.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        // Vertical Camera Rotation (Pitch)
        float verticalInput = Input.GetAxis("Mouse Y") * rotationSpeed;
        currentXRotation -= verticalInput * Time.deltaTime;
        currentXRotation = Mathf.Clamp(currentXRotation, -verticalRotationLimit, verticalRotationLimit); // Clamp

        // Rotate the CAMERA (child) - Local Rotation
        transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);


        // Position (Smooth Following) - No longer needed, camera is a child

    }
}