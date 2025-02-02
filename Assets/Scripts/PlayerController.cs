using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float climbSpeed = 3f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float originalHeight;
    private float crouchHeight = 1f;
    private bool canJump = true;
    private Animator animator;
    private Vector3 moveDirection;

    private bool isCrouching = false;
    private bool isClimbing = false;   
                                       

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        originalHeight = controller.height;
    }

    void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f; // Prevent floating effect

        float moveSpeed = walkSpeed;
        bool isRunning = false;

        // Movement Speed Adjustments (Run/Crouch)
        if (Input.GetKey(KeyCode.LeftShift)) { moveSpeed = runSpeed; isRunning = true; }
        else if (isCrouching) { moveSpeed = crouchSpeed; } // Maintain crouch speed when crouching

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate movement direction
        moveDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;

        if (moveDirection.magnitude < 0.01f) moveDirection = Vector3.zero;

        if (moveDirection != Vector3.zero && !isClimbing)
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);


        // Handle crouch toggle input (toggle when pressed once)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;  // Toggle crouch state
        }

        // Handle crouching
        HandleCrouching(isCrouching);
        // Handle climbing
        HandleClimbing();
        HandleJumping();
        HandleAnimations(isRunning, verticalInput);
    }

    void HandleJumping()
    {
        // Jumping only when grounded and not already in the air
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Jump velocity
            animator.SetBool("isJumping", true);
            canJump = false; // Prevent further jumps until grounded again
        }
        if (!isGrounded && !Input.GetKey(KeyCode.Space))
        {
            // Allow jumping again when grounded
            animator.SetBool("isJumping", false); // Reset jump animation
        }

        // Reset jump ability when grounded and not pressing jump
        if (isGrounded && !Input.GetKey(KeyCode.Space))
        {
            canJump = true;  // Allow jumping again when grounded
            animator.SetBool("isJumping", false); // Reset jump animation
        }
    }

    void HandleCrouching(bool isCrouching)
    {
        if (isCrouching)
        {
            // Adjust height and center when crouching to avoid clipping through the ground
            controller.height = crouchHeight;
            controller.center = new Vector3(controller.center.x, crouchHeight / 2, controller.center.z);
            animator.SetBool("isCrouching", true);
        }
        else
        {
            // Reset height and center when standing up
            controller.height = originalHeight;
            controller.center = new Vector3(controller.center.x, originalHeight / 2, controller.center.z);
            animator.SetBool("isCrouching", false);
        }
    }
    void HandleClimbing()
    {
        // Detect climbable surfaces
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            if (hit.collider.CompareTag("Climbable") && Input.GetKey(KeyCode.E))
            {
                isClimbing = true;
                animator.SetBool("isClimbing", true);
                velocity.y = 0; // Reset vertical velocity
            }
        }

        if (isClimbing)
        {
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 climbDirection = new Vector3(0, verticalInput, 0).normalized;
            controller.Move(climbDirection * climbSpeed * Time.deltaTime);

            if (verticalInput == 0)
            {
                animator.SetBool("isClimbing", false);
                isClimbing = false;
            }
        }
    }

    void HandleAnimations(bool isRunning, float verticalInput)
    {
        if (isGrounded)
        {
            // Handle Running
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isWalking", false);  // Ensure walking is false when running

            // Handle Walking
            if (!isRunning && moveDirection.magnitude > 0 && verticalInput >= 0 && !isCrouching)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isWalkingBack", false); // Ensure walking back animation is false
            }
            else if (moveDirection.magnitude > 0 && verticalInput < 0 &&!isCrouching )
            {
                animator.SetBool("isWalking", false); // Ensure walking animation is false when walking back
                animator.SetBool("isWalkingBack", true);
            }
            
            else
            {
                animator.SetBool("isWalking", false); // Ensure walking is false when idle
                animator.SetBool("isWalkingBack", false);
            }
            // Handle Crouch Walking
            if (isCrouching && moveDirection.magnitude > 0)
            {
                animator.SetBool("isWalkingCrouch", true);
                animator.SetBool("isCrouching", false);
                animator.SetBool("isWalking", false);

            }
            else
            {
                animator.SetBool("isWalkingCrouch", false);
                
            }

            // Handle Idle
            if (moveDirection.magnitude == 0 && !isCrouching)
            {
                animator.SetBool("isIdle", true);
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalkingBack", false);
                animator.SetBool("isWalkingCrouch", false);
            }
            else
            {
                animator.SetBool("isIdle", false);
            }
        }
    }

    void ApplyGravity()
    {
        // Apply gravity to velocity
        if (!isGrounded) velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
