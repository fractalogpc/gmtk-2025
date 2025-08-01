using UnityEngine;

public class CartController : MonoBehaviour
{
    public Transform player;

    public bool isCartActive = false;

    public void Interact()
    {
        if (isCartActive)
        {
            // Deactivate cart
            isCartActive = false;
        }
        else
        {
            // Activate cart
            isCartActive = true;
        }
    }

    void Update()
    {
        if (isCartActive)
        {

            if (Vector3.Distance(IgnoreY(transform.position), IgnoreY(player.position)) < 2f)
            {
                return;
            }
            // Slerp to look at the player
            Vector3 direction = IgnoreY(player.position - transform.position);
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            // If the cart is close to the player, don't move it
            if (Vector3.Distance(IgnoreY(transform.position), IgnoreY(player.position)) < 5f)
            {
                return;
            }

            // Follow the the player but stay a distance away to prevent collision
            Vector3 targetPosition = player.position - direction.normalized * 5f;
            targetPosition.y = transform.position.y; // Keep the cart at the same height
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
        }
    }

    private Vector3 IgnoreY(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
}
