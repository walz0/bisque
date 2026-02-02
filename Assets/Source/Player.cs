using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum PlayerState
    {
        WALK,
        ROLL,
        AIRBORNE,
        SLIDE
    }

    const float MAX_VELOCITY = 50f;
    const float MOVE_SPEED_ROLL = 500f;
    const float MOVE_SPEED_ROLL_STRAFE = 750f;
    const float MOVE_SPEED_WALK = 250f;
    const float ROT_SPEED = 1500f;
    const float JUMP_SPEED = 20f;
    const float MOVE_ACCEL = 1f;
    const float SLIDE_SPEED = 100f;
    const float ANGULAR_DRAG_FLAT = 3f;
    const float ANGULAR_DRAG_SLOPE = 0f;
    const float SLOPE_ANGLE = 5f;
    const int SLIDE_TIME = 30;
    const float ROTATE_SPEED = 250f;
    const float GRAVITY = 50f;

    private PlayerState currentState;
    private float throttle = 0;
    private bool grounded = false;
    private Vector3 groundPoint;
    private Vector3 groundNormal;
    private Vector2 inputVector;
    private PlayerCamera playerCamera;

    private int slideTimer = -1;

    private Rigidbody rb;
    [SerializeField]
    private GameObject lobsterObject;
    [SerializeField]
    private GameObject ballObject;
    private GroundCheck groundCheck;
    private Animator anim;

    void Start()
    {
        inputVector = new Vector2();
        rb = GetComponent<Rigidbody>();
        playerCamera = FindFirstObjectByType<PlayerCamera>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        anim = GetComponentInChildren<Animator>();

        InitPlayer();
    }

    void InitPlayer()
    {
        lobsterObject.transform.parent = null;
        groundCheck.transform.parent = null;
        groundCheck.SetPlayer(this);
        SetState(PlayerState.ROLL);
    }

    void ProcessInput()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        inputVector.Normalize();

        // Restart level
        if (Input.GetKeyDown(KeyCode.R))
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneIndex);
        }

        // Roll
        /*
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (GetState() != PlayerState.ROLL)
            {
                SetState(PlayerState.ROLL);
            }
            else
            {
                SetState(PlayerState.WALK);
            }
        }
        */

        /*
        // Slide
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Slide if there is sufficient butter
            SetState(PlayerState.SLIDE);
        }
        */

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsGrounded())
            {
                Jump();
            }
        }
    }

    void UpdateStateTimers()
    {
        if (slideTimer > -1)
        {
            slideTimer--;
        }
        if (slideTimer == 0)
        {
            OnSlideEnd();
        }
    }

    void OnStateChange(PlayerState oldState, PlayerState newState)
    {
        switch(newState)
        {
            case PlayerState.WALK:
                rb.freezeRotation = true;
                rb.linearDamping = 10f;
                break;
            case PlayerState.ROLL:
                rb.freezeRotation = false;
                rb.linearDamping = 0.1f;
                break;
            case PlayerState.SLIDE:
                slideTimer = SLIDE_TIME;
                break;
        }
    }

    void SetState(PlayerState newState)
    {
        OnStateChange(currentState, newState);
        currentState = newState;
    }

    PlayerState GetState()
    {
        return currentState;
    }

    void UpdateAnimator()
    {
        anim.SetFloat("ball_vel", rb.linearVelocity.magnitude / MAX_VELOCITY);
    }

    void UpdateVisuals()
    {
        Vector3 forward = GetForward();
        lobsterObject.transform.forward = forward;

        const float lobsterHeight = 1.4f;
        lobsterObject.transform.position = transform.position + Vector3.up * lobsterHeight;
    }

    void UpdateSlopeDrag()
    {
        float angle = Vector3.Angle(groundNormal, Vector3.up);
        rb.angularDamping = angle > SLOPE_ANGLE ? ANGULAR_DRAG_SLOPE : ANGULAR_DRAG_FLAT;
    }

    void Update()
    {
        ProcessInput();
        CheckGround();
        UpdateVisuals();
        UpdateAnimator();
        ApplyGravity();

        if (IsGrounded())
        {
            switch (currentState)
            {
                case PlayerState.WALK:
                    Walk();
                    break;
                case PlayerState.ROLL:
                    Roll();
                    break;
                case PlayerState.SLIDE:
                    Slide();
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateStateTimers();
    }

    void UpdateThrottle()
    {
        if (inputVector.y > 0.01f)
        {
            throttle += inputVector.y * MOVE_ACCEL;
        }
        else if (throttle > 0f)
        {
            throttle -= MOVE_ACCEL * 3f;
        }
        throttle = Mathf.Clamp(throttle, 0f, MOVE_SPEED_ROLL);
    }

    void ApplyGravity()
    {
        rb.AddForce(Vector3.down * GRAVITY * Time.deltaTime, ForceMode.Impulse);
    }

    bool IsRolling()
    {
        return GetState() == PlayerState.ROLL;
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * JUMP_SPEED, ForceMode.Impulse);
    }

    void Walk()
    {
        transform.rotation = Quaternion.identity;

        Vector3 forward = GetForward();
        Vector3 right = GetRight();
        rb.AddForce(forward * inputVector.y * MOVE_SPEED_WALK * Time.deltaTime, ForceMode.VelocityChange);
        rb.AddForce(right * inputVector.x * MOVE_SPEED_WALK * Time.deltaTime, ForceMode.VelocityChange);

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MAX_VELOCITY);
    }

    void Roll()
    {
        Vector3 forward = GetForward();
        Vector3 right = GetRight();
        rb.AddForce(forward * inputVector.y * MOVE_SPEED_ROLL * Time.deltaTime);

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, forward) * rb.linearVelocity.magnitude;
        float strafeMultiplier = forwardSpeed * 0.01f; // Make turning stronger at higher velocities
        rb.AddForce(right * inputVector.x * MOVE_SPEED_ROLL_STRAFE * Time.deltaTime);
        rb.AddTorque(forward * -inputVector.x * ROT_SPEED * Time.deltaTime);
        rb.AddTorque(right * inputVector.y * ROT_SPEED * Time.deltaTime);
        //transform.Rotate(Vector3.up * inputVector.x * ROTATE_SPEED * Time.deltaTime);

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MAX_VELOCITY);
    }

    public bool IsSliding()
    {
        return slideTimer > 0;
    }

    void OnSlideEnd()
    {
        SetState(PlayerState.ROLL);
    }

    void Slide()
    {
        Vector3 forward = GetCamForward();
        forward.y = 0;
        rb.AddForce(forward * SLIDE_SPEED * Time.deltaTime, ForceMode.Impulse);
    }
    
    bool IsGrounded()
    {
        return grounded;
    }

    void CheckGround()
    {
        const float checkDist = 0.5f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.down, Vector3.down, out hit, checkDist))
        {
            groundNormal = hit.normal;
            groundPoint = hit.point;
            grounded = true;
            SetState(PlayerState.ROLL);
        }
        else
        {
            grounded = false;
            SetState(PlayerState.AIRBORNE);
        }
    }

    public Vector2 GetInputVector()
    {
        return inputVector;
    }

    private Vector3 GetForward()
    {
        float orbitAngle = playerCamera.GetOrbitAngle();
        return new Vector3(
                Mathf.Cos(orbitAngle * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(orbitAngle * Mathf.Deg2Rad)
            ); 
    } 
    
    private Vector3 GetRight()
    {
        Vector3 right = GetCamRight();
        right.y = 0;
        return right;
    } 

    private Vector3 GetCamForward()
    {
        if (playerCamera)
        {
            return playerCamera.transform.forward;
        }
        return Vector3.zero;
    }

    private Vector3 GetCamRight()
    {
        if (playerCamera)
        {
            return playerCamera.transform.right;
        }
        return Vector3.zero;
    }

    public void SetGrounded(bool is_grounded)
    {
        grounded = is_grounded;
    }

    public Vector3 GetLobsterPos()
    {
        return lobsterObject.transform.position;
    }
}
