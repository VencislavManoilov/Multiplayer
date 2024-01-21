using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float fallMultiplier = 2f;
    public float sensitivity = 2f;

    private bool isGrounded;
    private float cameraRotationX = 0f;
    private Rigidbody rb;

    public float ms;
    private server server;
    private float time = 0;

    void Start() {
        server = FindFirstObjectByType<server>();
        ms /= 2;

        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        HandleInput();

        if (rb.velocity.y < 0) {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    void FixedUpdate() {
        time += 0.5f;
        if(time%ms == 0) {
            server.SendPosition(transform.position, new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z));
        }
    }

    private void HandleInput() {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 movementDirection = (forward * verticalInput + right * horizontalInput).normalized;

        Vector3 movement = movementDirection * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        if (isGrounded && Input.GetButtonDown("Jump")) {
            Jump();
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        transform.Rotate(Vector3.up * mouseX);

        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null) {
            cameraRotationX -= mouseY;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -90f, 90f);

            cam.transform.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
        }
    }

    private void Jump() {
        float jumpForce = Mathf.Sqrt(2 * Physics.gravity.magnitude * jumpHeight);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        isGrounded = false;
    }

    private void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            isGrounded = true;
        }
    }
}