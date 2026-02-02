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

    private Vector3 DEFAULT_CAMERA_POS = new Vector3(0, 4f, -6.5f);

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

    private float orbit_angle = 0f;

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

        const float orbitSpeed = 150f;
        orbit_angle += -player.GetInputVector().x * orbitSpeed * Time.deltaTime;

        float follow_dist = player.GetState() == Player.PlayerState.SLIDE ? Mathf.Abs(DEFAULT_CAMERA_POS.z) * 0.75f : Mathf.Abs(DEFAULT_CAMERA_POS.z);
        float rad = orbit_angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(rad),
            0f,
            Mathf.Sin(rad)
        ) * follow_dist;

        Vector3 player_pos = player.transform.position;
        transform.position = Vector3.Lerp(transform.position, player_pos - offset + Vector3.up * 3f, Time.deltaTime * 10f);
        transform.LookAt(player_pos + Vector3.up * 1.6f);

        /*
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

        float follow_dist = Mathf.Abs(DEFAULT_CAMERA_POS.z);
        Vector3 player_pos = player.transform.position;
        float dist_to_player = Vector3.Distance(transform.position, player_pos);
        */



        /*
         * - Take the camera's initial forward vector and use it to place the camera behind the player
         * - Set the camera follow_dist away from the player
         * - Look at player
        */

        /*
        const float lobsterHeight = 1.4f;
        transform.LookAt(player_pos + Vector3.up * lobsterHeight);

        //if (dist_to_player > follow_dist)
        {
            Vector3 cam_forward_flat = new Vector3(transform.forward.x, 0, transform.forward.z);
            transform.position = player_pos + -Vector3.forward * follow_dist + Vector3.up * DEFAULT_CAMERA_POS.y;
        }
        */

        //Quaternion rotation = Quaternion.Euler(pitch, yaw, roll);
        //Vector3 offset = rotation * new Vector3(0f, 0f, DEFAULT_CAMERA_POS.z);
        //transform.position = player_pos + offset;


        //transform.position = Vector3.Lerp(transform.position, player_pos + offset, Time.deltaTime);

        //Vector3 look_target = new Vector3(player_pos.x, player.GetLobsterPos().y + 0.5f, player_pos.z);

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

    public float GetOrbitAngle()
    {
        return orbit_angle;
    }
}
