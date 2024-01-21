using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;

    private bool isGrounded;
    private Rigidbody rb;

    public float ms;
    private server server;
    private float time = 0;

    void Start() {
        server = FindFirstObjectByType<server>();
        ms /= 2;

        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        HandleInput();
    }

    void FixedUpdate() {
        time += 0.5f;
        if(time%ms == 0) {
            server.SendPosition(transform.position);
        }
    }

    private void HandleInput() {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput) * moveSpeed * Time.deltaTime;

        transform.Translate(movement, Space.World);

        if(isGrounded && Input.GetButtonDown("Jump")) {
            Jump();
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