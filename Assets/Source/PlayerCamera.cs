using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    const float CAM_MAX_PITCH_ANGLE = 89f;
    const float CAM_MAX_ADD_SPEED = 8.5f;
    const float CAM_MAX_ROLL_ANGLE = 0.25f;
    const float CAM_BASE_FOV = 60f;
    const float BASE_SPEED = 100f;
    const float MIN_SHAKE_DIST = 0.1f;
    const float MAX_SHAKE_DIST = 1f;

    private Vector3 DEFAULT_CAMERA_POS = new Vector3(0, 4f, -8.5f);

    private float mouse_x = 0f;
    private float mouse_y = 0f;

    private float pitch_raw = 0f;
    private float yaw_raw = 0f;
    private float roll_raw = 0f;

    private float pitch = 0f;
    private float yaw = 0f;
    private float roll = 0f;

    private float pitch_add = 0f;
    private float yaw_add = 0f;
    private float roll_add = 0f;

    private float pitch_target = 0f;
    private float yaw_target = 0f;
    private float roll_target = 0f;

    private Vector3 orbit_offset;
    private Vector3 target_position;

    public float sensitivity = 1f;

    private float shake_amount = 0f;
    private int shake_frames = -1;

    private Player player;

    bool cursorLocked = true;

    void Start()
    {
        player = FindFirstObjectByType<Player>();
        orbit_offset = DEFAULT_CAMERA_POS;
    }

    void Update()
    {
        UpdateShake();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = false;
        }

        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        mouse_x = Input.GetAxisRaw("Mouse X");
        mouse_y = Input.GetAxisRaw("Mouse Y");

        pitch_raw += -mouse_y * sensitivity;
        pitch_raw = Mathf.Clamp(pitch_raw, -CAM_MAX_PITCH_ANGLE, CAM_MAX_PITCH_ANGLE);
        yaw_raw += mouse_x * sensitivity;

        float playerVel = player.GetComponent<Rigidbody>().linearVelocity.magnitude;
        // float fovScale = (playerVel * 0.005f) + 1f;
        GetComponent<Camera>().fieldOfView = CAM_BASE_FOV;

        pitch_add = Mathf.LerpAngle(pitch_add, pitch_target, Time.deltaTime * CAM_MAX_ADD_SPEED);
        yaw_add = Mathf.LerpAngle(yaw_add, yaw_target, Time.deltaTime * CAM_MAX_ADD_SPEED);
        roll_add = Mathf.LerpAngle(roll_add, roll_target, Time.deltaTime * CAM_MAX_ADD_SPEED);

        pitch = pitch_raw + pitch_add;
        yaw = yaw_raw + yaw_add;
        roll = roll_raw + roll_add;

        float follow_dist = DEFAULT_CAMERA_POS.z;
        Vector3 player_pos = player.transform.position;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, DEFAULT_CAMERA_POS.z);
        transform.position = player_pos + offset;

        //orbit_offset = Quaternion.AngleAxis(mouse_x * sensitivity, Vector3.up) * Quaternion.AngleAxis(mouse_y * sensitivity, Vector3.right) * orbit_offset;
        //transform.position = player_pos + orbit_offset;

        /*
        // Calculate default camera position relative to the player's facing direction
        target_position = player_pos + new Vector3(
                -player.transform.forward.x * -DEFAULT_CAMERA_POS.z,
                DEFAULT_CAMERA_POS.y,
                -player.transform.forward.z * -DEFAULT_CAMERA_POS.z
            );

        // Lerp camera toward target position
        transform.position = Vector3.Lerp(
                transform.position,
                target_position,
                Vector3.Distance(target_position, transform.position) * Time.deltaTime * 2.5f
            );
        */

        Vector3 look_target = player_pos + new Vector3(0, 1f, 0);
        transform.LookAt(look_target);

        //Vector3 player_vel = player.GetComponent<Rigidbody>().linearVelocity;
        //float fov_scale = (player_vel.magnitude * 0.005f) + 1f;
        //GetComponent<Camera>().fieldOfView = CAM_BASE_FOV * fov_scale;
    }

    private void FixedUpdate()
    {
        if (shake_frames > -1)
        {
            shake_frames--;
        }
        if (shake_frames == 0)
        {
            OnShakeEnd();
        }
    }

    void OnShakeEnd()
    {
        transform.localPosition = DEFAULT_CAMERA_POS;
    }

    void UpdateShake()
    {
        if (shake_frames > 0)
        {
            Vector3 shakeDistance = new Vector3(
                    Random.Range(MIN_SHAKE_DIST, MAX_SHAKE_DIST) * shake_amount,
                    Random.Range(MIN_SHAKE_DIST, MAX_SHAKE_DIST) * shake_amount,
                    0f
                );
            transform.localPosition = DEFAULT_CAMERA_POS + shakeDistance;
        }
    }

    public void ApplyScreenShake(int frames, float amount = 1f)
    {
        shake_frames = frames;
        shake_amount = amount;
    }

    public void SetTargetPitch(float pitchTarget)
    {
        pitch_target = pitchTarget;
    }
    public void SetTargetRoll(float rollTarget)
    {
        roll_target = rollTarget;
    }
}
