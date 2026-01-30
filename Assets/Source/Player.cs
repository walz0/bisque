using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    const float MAX_VELOCITY = 10f;
    const float MOVE_SPEED = 100f;
    const float MOVE_ACCEL = 1f;
    const float SLIDE_SPEED = 500f;
    const int SLIDE_TIME = 30;
    const float ROTATE_SPEED = 250f;

    private float throttle = 0;
    private Vector2 inputVector;

    private int slideTimer = -1;

    private Rigidbody rb;

    void Start()
    {
        inputVector = new Vector2();
        rb = GetComponent<Rigidbody>();
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

        // Slide
        if (Input.GetKeyDown(KeyCode.Space))
        {
            slideTimer = SLIDE_TIME;
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

    void Update()
    {
        ProcessInput();
        if (IsSliding())
        {
            Slide();
        }
        else
        {
            UpdateThrottle();
        }
        Move();
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
        throttle = Mathf.Clamp(throttle, 0f, MOVE_SPEED);
    }

    void Move()
    {
        rb.AddRelativeForce(Vector3.forward * throttle * Time.deltaTime, ForceMode.VelocityChange);
        transform.Rotate(Vector3.up * inputVector.x * ROTATE_SPEED * Time.deltaTime);

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MAX_VELOCITY);
    }

    public bool IsSliding()
    {
        return slideTimer > 0;
    }

    void OnSlideEnd()
    {

    }

    void Slide()
    {
        throttle = SLIDE_SPEED;
    }

    public Vector2 GetInputVector()
    {
        return inputVector;
    }
}
