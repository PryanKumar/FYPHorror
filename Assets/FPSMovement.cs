using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPSMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrain = 25f;
    public float staminaRegen = 15f;
    private bool isExhausted = false;

    [Header("Breathing SFX")]
    public AudioSource breathingSource;
    public float breathingFadeSpeed = 2f; // How fast the volume fades in/out
    private bool isBreathing = false;
    private float targetBreathingVolume = 0f;

    [Header("Jumping & Gravity")]
    public float jumpForce = 5f;
    public float gravityMultiplier = 3.5f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Height Settings")]
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSmoothTime = 0.1f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Vector3 moveInput;
    private float targetHeight;
    private float currentHeightVelocity;
    private bool isGrounded;
    private bool wasGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        currentStamina = maxStamina;
        targetHeight = standingHeight;

        // Initialize Audio
        if (breathingSource != null)
        {
            breathingSource.loop = true;
            breathingSource.volume = 0f; // Start silent
            breathingSource.Play();      // Keep playing but silent
        }
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(x, 0, z);

        HandleStamina();
        HandleCrouchInput();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Smoothly fade the breathing volume every frame
        if (breathingSource != null)
        {
            breathingSource.volume = Mathf.MoveTowards(breathingSource.volume, targetBreathingVolume, breathingFadeSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        ApplyMovement();
        ApplyGravity();
        ApplySmoothCrouch();

        if (isGrounded && !wasGrounded)
        {
            OnLand();
        }
        wasGrounded = isGrounded;
    }

    void HandleStamina()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && moveInput.magnitude > 0 && isGrounded;

        if (isSprinting && !isExhausted)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina <= 0) isExhausted = true;
        }
        else
        {
            currentStamina += staminaRegen * Time.deltaTime;
            if (isExhausted && currentStamina >= 25f) isExhausted = false;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // --- Breathing Fade Logic ---
        float staminaPercent = (currentStamina / maxStamina) * 100f;

        // Trigger fade in at 70%
        if (!isBreathing && staminaPercent <= 70f)
        {
            isBreathing = true;
            targetBreathingVolume = 1f;
        }
        // Trigger fade out at 80%
        else if (isBreathing && staminaPercent >= 80f)
        {
            isBreathing = false;
            targetBreathingVolume = 0f;
        }
    }

    void ApplyMovement()
    {
        float speed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && !isExhausted && !Input.GetKey(KeyCode.LeftControl))
            speed = sprintSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            speed = crouchSpeed;

        Vector3 moveDirection = rb.rotation * new Vector3(moveInput.x, 0, moveInput.z);

        if (moveDirection.magnitude > 1) moveDirection.Normalize();

        Vector3 targetVel = moveDirection * speed;
        targetVel.y = rb.linearVelocity.y;

        rb.linearVelocity = targetVel;
    }

    void ApplyGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    void HandleCrouchInput() => targetHeight = Input.GetKey(KeyCode.LeftControl) ? crouchHeight : standingHeight;

    void ApplySmoothCrouch() => col.height = Mathf.SmoothDamp(col.height, targetHeight, ref currentHeightVelocity, crouchSmoothTime);

    void OnLand()
    {
        MouseLook ml = GetComponentInChildren<MouseLook>();
        if (ml != null) ml.TriggerLandShake();
    }
}