using UnityEngine;

public class PlayAnimation : MonoBehaviour
{

    [SerializeField] private Animation anim;

    public void Play()
    {
        if (anim != null)
        {
            anim.Play();
        }
        else
        {
            Debug.LogWarning("Animation is not assigned in PlayAnimation script.");
        }
    }
    
}
