using UnityEngine;
using System.Collections;

public class SpinningAdvanced : MonoBehaviour
{

    [SerializeField] private AnimationCurve spinCurve;
    [SerializeField] private float spinSpeed = 100f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private Vector3 spinAxis = Vector3.up;

    private Quaternion initialRotation;
    private float angle = 0f;

    void OnEnable()
    {
        initialRotation = transform.localRotation;
        StartCoroutine(SpinCycle());
    }

    private IEnumerator SpinCycle()
    {
        while (true)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                angle += spinCurve.Evaluate(t) * spinSpeed * Time.deltaTime;
                transform.localRotation = initialRotation * Quaternion.Euler(spinAxis * angle);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

}
