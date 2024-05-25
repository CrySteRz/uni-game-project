using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    public float walkSpeed = 2.0f;
    [SerializeField]
    public float runSpeed = 6.0f;
    [SerializeField]
    public float rotationSpeed;
    [SerializeField]
    public float jumpSpeed;
    [SerializeField]
    public float jumpButtonGracePeriod;
    [SerializeField]
    private float jumpHorizontalSpeed;
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private Transform holdPosition;
    [SerializeField]
    private float pickUpRange = 2.0f;
    [SerializeField]
    private Transform seatPosition;
    [SerializeField]
    private GameObject sweepo;
    [SerializeField]
    private float enterCarRange = 2.0f;

    private Animator animator;
    private CharacterController characterController;
    private float ySpeed;
    private float originalStepOffset;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;

    private bool isJumping;
    private bool isGrounded;
    private bool isDriving = false; // To check if the player is driving
    private GameObject heldObject;

    private GameObject potentialPickUpObject;
    private Collider playerCollider;

    private SweepoController sweepoController;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        playerCollider = GetComponent<Collider>();
        originalStepOffset = characterController.stepOffset;

        // Get the SweepoController component
        if (sweepo != null)
        {
            sweepoController = sweepo.GetComponent<SweepoController>();
        }

        // Ensure seat position is assigned
        if (seatPosition == null)
        {
            Debug.LogError("Seat Position is not assigned!");
        }
    }

    void Update()
    {
        if (!isDriving)
        {
            HandleMovement();
        }

        // Check for E key press to pick up or drop the garbage bag
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null && potentialPickUpObject != null && Vector3.Distance(transform.position, potentialPickUpObject.transform.position) <= pickUpRange)
            {
                TryPickUp();
            }
            else if (heldObject != null)
            {
                Drop();
            }
        }

        // Handle sweepo entry/exit
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isDriving)
            {
                ExitCar();
            }
            else if (Vector3.Distance(transform.position, sweepo.transform.position) <= enterCarRange)
            {
                EnterCar();
            }
        }

        // Reset isPicking after the pick up animation is done
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Pick Up"))
        {
            animator.SetBool("isPicking", false);
        }
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        // Check if Shift key is pressed
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;

        float animationMagnitude = isRunning ? inputMagnitude : inputMagnitude * 0.5f;
        animator.SetFloat("Input_Magnitude", animationMagnitude, 0.05f, Time.deltaTime);

        movementDirection.Normalize();

        ySpeed += Physics.gravity.y * Time.deltaTime;

        if (characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpButtonPressedTime = Time.time;
        }

        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;
            animator.SetBool("isGrounded", true);
            isGrounded = true;
            animator.SetBool("isJumping", false);
            isJumping = false;
            animator.SetBool("isFalling", false);

            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = jumpSpeed;
                animator.SetBool("isJumping", true);
                isJumping = true;
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
        }
        else
        {
            characterController.stepOffset = 0;
            animator.SetBool("isGrounded", false);
            isGrounded = false;

            if ((isJumping && ySpeed < 0) || ySpeed < -2)
            {
                animator.SetBool("isFalling", true);
            }
        }

        if (movementDirection != Vector3.zero)
        {
            animator.SetBool("isMoving", true);
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        if (isGrounded == false)
        {
            Vector3 velocity = movementDirection * inputMagnitude * jumpHorizontalSpeed;
            velocity.y = ySpeed;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    void EnterCar()
    {
        Debug.Log("Entering Car");
        Debug.Log("Seat Position: " + seatPosition.position);
        Debug.Log("Player Position Before: " + transform.position);

        // Ensure seat position is assigned
        if (seatPosition != null)
        {
            characterController.enabled = false; // Disable CharacterController

            // Reset movement parameters and set to idle
            animator.SetFloat("Input_Magnitude", 0);
            animator.SetBool("isMoving", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", true); // Assume the player is grounded when entering the sweepo

            transform.position = seatPosition.position;
            transform.rotation = seatPosition.rotation;
            transform.parent = sweepo.transform;

            Debug.Log("Player Position After: " + transform.position);

            if (sweepoController != null)
            {
                sweepoController.StartDriving(); // Notify the sweepo to start driving
            }
        }
        else
        {
            Debug.LogError("Seat Position is not assigned!");
        }

        isDriving = true;
        animator.SetBool("isDriving", true);
    }
    public bool IsDriving()
    {
        return isDriving;
    }

    void ExitCar()
    {
        Debug.Log("Exiting Car");
        transform.parent = null;

        characterController.enabled = true; // Re-enable CharacterController

        if (sweepoController != null)
        {
            sweepoController.StopDriving(); // Notify the sweepo to stop driving
        }

        isDriving = false;
        animator.SetBool("isDriving", false);
    }

    void TryPickUp()
    {
        if (potentialPickUpObject != null)
        {
            Debug.Log("Preparing to pick up: " + potentialPickUpObject.name);
            animator.SetBool("isPicking", true);
        }
    }

    public void AttachObject()
    {
        if (potentialPickUpObject != null)
        {
            Debug.Log("Picking up: " + potentialPickUpObject.name);
            heldObject = potentialPickUpObject;
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
            heldObject.GetComponent<Collider>().enabled = false; // Disable the collider
            heldObject.transform.SetParent(holdPosition);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;
            potentialPickUpObject = null; // Clear the potential pick-up object
        }
    }

    void Drop()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject.GetComponent<Collider>().enabled = true; // Re-enable the collider
            heldObject = null;
        }
    }

    private void OnAnimatorMove()
    {
        if (isGrounded)
        {
            Vector3 velocity = animator.deltaPosition;
            velocity.y = ySpeed * Time.deltaTime;

            characterController.Move(velocity);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Pickable"))
        {
            Debug.Log("Collided with: " + hit.collider.name);
            potentialPickUpObject = hit.collider.gameObject;
        }
    }
}
