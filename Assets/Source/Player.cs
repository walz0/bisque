using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum PlayerState
    {
        WALK,
        ROLL,
        SLIDE
    }

    const float MAX_VELOCITY = 10f;
    const float MOVE_SPEED_ROLL = 10000f;
    const float MOVE_SPEED_WALK = 150f;
    const float ROT_SPEED = 1500f;
    const float MOVE_ACCEL = 1f;
    const float SLIDE_SPEED = 100f;
    const int SLIDE_TIME = 30;
    const float ROTATE_SPEED = 250f;
    const float GRAVITY = 50f;

    private PlayerState currentState;
    private float throttle = 0;
    private bool grounded = false;
    private Vector2 inputVector;
    private PlayerCamera playerCamera;

    private int slideTimer = -1;

    private Rigidbody rb;
    [SerializeField]
    private GameObject lobsterObject;

    void Start()
    {
        inputVector = new Vector2();
        rb = GetComponent<Rigidbody>();
        playerCamera = FindFirstObjectByType<PlayerCamera>();

        lobsterObject.transform.parent = null;

        SetState(PlayerState.WALK);
    }

    void ProcessInput()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        inputVector.Normalize();

        //playerCamera.SetTargetRoll(inputVector.x * 10f);
        //playerCamera.SetTargetPitch(inputVector.y * 10f);

        // Restart level
        if (Input.GetKeyDown(KeyCode.R))
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneIndex);
        }

        // Roll
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

        // Slide
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Slide if there is sufficient butter
            SetState(PlayerState.SLIDE);
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

    void Update()
    {
        ProcessInput();
        ApplyGravity();

        print(IsGrounded());

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

    void Walk()
    {
        transform.rotation = Quaternion.identity;

        Vector3 forward = GetCamForward();
        forward.y = 0;
        Vector3 right = GetCamRight();
        right.y = 0;
        rb.AddForce(forward * inputVector.y * MOVE_SPEED_WALK * Time.deltaTime, ForceMode.VelocityChange);
        rb.AddForce(right * inputVector.x * MOVE_SPEED_WALK * Time.deltaTime, ForceMode.VelocityChange);

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MAX_VELOCITY);
    }

    void Roll()
    {
        Vector3 forward = GetCamForward();
        forward.y = 0;
        Vector3 right = GetCamRight();
        right.y = 0;
        rb.AddForce(forward * inputVector.y * MOVE_SPEED_ROLL * Time.deltaTime);
        rb.AddForce(right * inputVector.x * MOVE_SPEED_ROLL * Time.deltaTime);
        rb.AddTorque(forward * -inputVector.x * ROT_SPEED * Time.deltaTime);
        rb.AddTorque(right * inputVector.y * ROT_SPEED * Time.deltaTime);
        //transform.Rotate(Vector3.up * inputVector.x * ROTATE_SPEED * Time.deltaTime);

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MAX_VELOCITY);

        lobsterObject.transform.forward = forward;
        lobsterObject.transform.position = transform.position + Vector3.up * 1.6f;
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

    public Vector2 GetInputVector()
    {
        return inputVector;
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
}
