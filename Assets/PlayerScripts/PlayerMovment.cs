using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Rigidbody component for physics-based movement.
    public Rigidbody rb;
    // Transform of the camera for orientation.
    public Transform cam;

    // Walking speed of the player.
    public float walkSpeed = 6f;
    // Running speed of the player.
    public float runSpeed = 12f;
    // Current speed of the player.
    public float speed;

    // Animator component for character animations.
    public Animator animator;

    // Time taken to smooth out the player's rotation.
    public float turnSmoothTime = 0.1f;
    // Velocity used for smoothing the player's rotation.
    private float turnSmoothVelocity;

    // Drag applied to the player when grounded.
    public float groundDrag = 5f;
    // Drag applied to the player when in the air.
    public float airDrag = 0.1f;

    // Direction in which the player is moving.
    private Vector3 moveDir;
    // Flag indicating if the player is grounded.
    private bool isGrounded;

    [Header("Matias")]
    // Reference to the MatiasMovement script.
    public MatiasMovement matiasSystem;

    // Animation parameter hash for speed.
    private readonly int speedHash = Animator.StringToHash("Speed");
    // Animation parameter hash for isRunning.
    private readonly int isRunningHash = Animator.StringToHash("isRunning");

    // Multiplier for slow effect.
    private float slowMultiplier = 1f;

    // Current move speed.
    public float moveSpeed;
    // Current speed after applying slow effects.
    public float currentSpeed;

    void Start()
    {
        // Get the Rigidbody component attached to the GameObject.
        rb = GetComponent<Rigidbody>();
        // Freeze the rotation of the Rigidbody.
        rb.freezeRotation = true;
        // Enable gravity for the Rigidbody.
        rb.useGravity = true;

        // Set initial speeds.
        moveSpeed = walkSpeed;
        currentSpeed = moveSpeed;

        // Initialize animation parameters.
        animator.SetFloat(speedHash, 0f);
        animator.SetBool(isRunningHash, false);
    }

    void Update()
    {
        // Check if the player is grounded.
        isGrounded = Physics.CheckSphere(transform.position, 0.5f, LayerMask.GetMask("Ground"));

        // Adjust drag based on whether the player is grounded.
        rb.linearDamping = isGrounded ? groundDrag : airDrag;

        // Get input from the player.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        // Normalize the direction vector.
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Check if the player is running.
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        // Set the speed based on whether the player is running or walking.
        speed = isRunning ? runSpeed : walkSpeed;
        moveSpeed = speed;

        // If no slow effect is active, update currentSpeed accordingly.
        if (slowMultiplier == 1f)
        {
            currentSpeed = moveSpeed;
        }

        // Update animation parameters based on movement speed and running state.
        float movementSpeed = direction.magnitude * currentSpeed;
        animator.SetFloat(speedHash, movementSpeed);
        animator.SetBool(isRunningHash, isRunning);

        // Handle rotation and movement direction.
        if (direction.magnitude >= 0.1f)
        {
            // Calculate the target rotation angle based on camera orientation.
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            // Smoothly rotate the player towards the target angle.
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            // Apply the rotation to the player.
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            // Calculate the movement direction based on the target angle.
            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            // Set the movement direction to zero if there is no input.
            moveDir = Vector3.zero;
        }

        // Notify Matias system when speed changes.
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (matiasSystem != null)
                matiasSystem.ResetSpeedSmoothing();
        }
    }

    // Apply a slow effect to the player.
    public void ApplySlow(float amount)
    {
        // 'amount' should be between 0 and 1 (e.g., 0.5 for 50% slow).
        slowMultiplier = 1f - amount;
        currentSpeed = moveSpeed * slowMultiplier;
    }

    // Remove the slow effect from the player.
    public void RemoveSlow()
    {
        slowMultiplier = 1f;
        currentSpeed = moveSpeed;
    }

    void FixedUpdate()
    {
        // Apply movement using physics. Only update the horizontal velocity.
        Vector3 horizontalVelocity = moveDir * currentSpeed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a green wire sphere to visualize the ground check radius.
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}