using UnityEngine;

public class SheepAnimation : MonoBehaviour
{
    public Animator animator;
    public AdvancedSheepController sheepController;
    
    void Update()
    {
        animator.SetBool("Moving", sheepController.moving);
        animator.SetBool("Running", sheepController.running);
    }
}
