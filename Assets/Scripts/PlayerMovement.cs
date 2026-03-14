using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D myBody;
    private SpriteRenderer sr;
    private PlayerControls controls;

    [Header("Basic Movement")]
    [SerializeField]
    private float moveForce = 6f;

    [Tooltip("How quickly the ship accelerates to full speed. Lower = sluggish, Higher = snappy.")]
    public float acceleration = 5f;

    [Tooltip("How quickly the ship slows down when no input. Lower = longer ocean drift.")]
    public float deceleration = 1.5f;

    public float moveX;
    public float moveY;
    public bool isGrounded = true;
    public bool canMove = true;
    private Vector2 lastMoveDirection = Vector2.right;

    [Header("Relative Movement")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Time Slow")]
    public bool canTimeSlow = false;
    public float slowDuration = 2f;
    public float slowFactor = 0.5f;
    public float slowFactorPlayer = 1f;
    public bool isSlowing = false;
    private float slowTimer = 0f;

    [Header("Roll")]
    public bool canRoll = true;
    public float rollForce = 15f;
    public float rollDuration = 0.25f;
    private float rollTimer = 0.3f;
    public float rollCooldown = 0.5f;
    public bool isRolling = false;
    public float rollCooldownTimer = 0.4f;
    private new CapsuleCollider2D collider; // 'new' suppresses CS0108 warning
    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;
    public Vector2 rollColliderSize = new Vector2(0f, 0f);
    public Vector2 rollColliderOffset = new Vector2(0f, 0f);
    private Vector2 rollDirection;

    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        controls = GetComponent<PlayerControls>();
        collider = GetComponent<CapsuleCollider2D>();
        normalColliderSize = collider.size;
        normalColliderOffset = collider.offset;

        myBody.gravityScale = 0f;
        myBody.freezeRotation = true;
    }

    void Start() { }

    void Update()
    {
        if (canMove && !isRolling)
            PlayerMoveKeyboard();

        ApplyRotation();

        // TIME SLOW
        if (canTimeSlow && !isSlowing && controls.fire3Pressed)
            StartTimeSlow();

        if (isSlowing)
        {
            slowTimer -= Time.unscaledDeltaTime;
            if (slowTimer <= 0f)
                EndTimeSlow();
        }

        // ROLL
        if (canRoll && !isRolling && rollCooldownTimer <= 0f && controls.rollPressed && canMove)
            StartRoll();

        if (isRolling)
        {
            rollTimer -= Time.unscaledDeltaTime;
            if (rollTimer <= 0f)
                EndRoll();
        }

        if (rollCooldownTimer > 0f)
            rollCooldownTimer -= Time.unscaledDeltaTime;
    }

    private void FixedUpdate()
    {
        if (isRolling)
            myBody.linearVelocity = rollDirection * rollForce;
    }

    void PlayerMoveKeyboard()
    {
        if (isRolling) return;

        moveX = controls.horizontalInput;
        moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

        if (moveDirection != Vector2.zero)
            lastMoveDirection = moveDirection;

        float topSpeed = moveForce;
        if (isSlowing)
            topSpeed *= slowFactorPlayer;

        if (moveDirection != Vector2.zero)
        {
            // Accelerate toward the target velocity — feels like pushing through water
            Vector2 targetVelocity = moveDirection * topSpeed;
            myBody.linearVelocity = Vector2.Lerp(
                myBody.linearVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            // No input — coast to a stop slowly (ocean drift)
            myBody.linearVelocity = Vector2.Lerp(
                myBody.linearVelocity,
                Vector2.zero,
                deceleration * Time.deltaTime
            );
        }
    }

    void ApplyRotation()
    {
        if (moveX != 0 || moveY != 0)
        {
            float targetAngle = Mathf.Atan2(moveY, moveX) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.unscaledDeltaTime);
            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
        }
    }

    void StartTimeSlow()
    {
        isSlowing = true;
        slowTimer = slowDuration;
        moveForce /= slowFactorPlayer;
        Time.timeScale = slowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void EndTimeSlow()
    {
        isSlowing = false;
        moveForce *= slowFactorPlayer;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        rollCooldownTimer = rollCooldown;

        float h = controls.horizontalInput;
        float v = Input.GetAxisRaw("Vertical");
        rollDirection = new Vector2(h, v).normalized;

        if (rollDirection == Vector2.zero)
            rollDirection = lastMoveDirection;

        collider.size = rollColliderSize;
        collider.offset = rollColliderOffset;
        canMove = false;
    }

    void EndRoll()
    {
        isRolling = false;
        canMove = true;
        collider.size = normalColliderSize;
        collider.offset = normalColliderOffset;
        myBody.linearVelocity = Vector2.zero;
    }
}