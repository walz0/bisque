using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private bool grounded = false;
    private Player player;

    private void Start()
    {
        player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        grounded = true;
        player.SetGrounded(grounded);
    }

    private void OnTriggerStay(Collider other)
    {
        grounded = true;
        player.SetGrounded(grounded);
    }

    private void OnTriggerExit(Collider other)
    {
        grounded = false;
        player.SetGrounded(grounded);
    }
}
