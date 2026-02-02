using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private bool grounded = false;
    private Player player;

    private void Start()
    {
    }

    public void SetPlayer(Player playerRef)
    {
        player = playerRef;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!player) return;
        grounded = true;
        player.SetGrounded(grounded);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!player) return;
        grounded = true;
        player.SetGrounded(grounded);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!player) return;
        grounded = false;
        player.SetGrounded(grounded);
    }
}
