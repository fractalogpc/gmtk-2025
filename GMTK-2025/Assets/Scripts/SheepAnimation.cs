using UnityEngine;

public class SheepAnimation : MonoBehaviour
{
    public Animator animator;
    public AdvancedSheepController sheepController;

    public Transform playerPosition;

    public float distanceThreshold = 50.0f;

    private bool shouldAnimate = false;

    public void TestForAnimation()
    {
        if (Vector3.Distance(playerPosition.position, transform.position) < distanceThreshold)
        {
            animator.enabled = true;
            animator.SetBool("Moving", sheepController.moving);
            animator.SetBool("Running", sheepController.running);
            shouldAnimate = true;
        }
        else
        {
            animator.SetBool("Moving", false);
            animator.SetBool("Running", false);
            animator.enabled = false;
            shouldAnimate = false;
        }

    }

    void Update()
    {
        if (!shouldAnimate) return;

        animator.SetBool("Moving", sheepController.moving);
        animator.SetBool("Running", sheepController.running);
    }
}
