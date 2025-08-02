using UnityEngine;

public class ClockManager : MonoBehaviour
{
    [SerializeField] private RectTransform clockHand;
    [SerializeField] private float dayStartDegree = 0f; // sunrise
    [SerializeField] private float dayEndDegree = 360f; // sunset

    public void UpdateClock(float timeLeft, float timeMax, bool offerTimeOverride)
    {
        if (clockHand == null)
        {
            Debug.LogWarning("Clock hand RectTransform is not assigned.");
            return;
        }
        
        if (offerTimeOverride)
        {
            clockHand.rotation = Quaternion.Euler(0f, 0f, 180f);
            return;
        }

        float currentDegree = Mathf.Lerp(dayStartDegree, dayEndDegree, 1 - (timeLeft / timeMax));
        clockHand.rotation = Quaternion.Euler(0f, 0f, currentDegree);
    }
}
