using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(CanvasGroup))]
public class FadeElementInOut : MonoBehaviour
{
  [Header("References")]
  public CanvasGroup _canvasGroup;
  [Header("Settings")]
  [SerializeField] private AnimationCurve _easingCurve;
  [SerializeField] private float _inDuration;
  [SerializeField] private float _outDuration;
  [Header("Events")]
  public UnityEvent _OnFadeComplete;
  
  public bool Shown => Mathf.Approximately(_canvasGroup.alpha, 1f);
  public bool Hidden => Mathf.Approximately(_canvasGroup.alpha, 0f);
  public bool FadingIn { get; private set; }
  public bool FadingOut { get; private set; }
  public float FadeInTime => _inDuration;
  public float FadeOutTime => _outDuration;

  private Coroutine _fadeCoroutine;
  
  void OnValidate() {
    if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
  }

  public void Hide() {
    _canvasGroup.alpha = 0;
    _canvasGroup.interactable = false;
    _canvasGroup.blocksRaycasts = false;
  }
  
  public void Show() {
    _canvasGroup.alpha = 1;
    _canvasGroup.interactable = true;
    _canvasGroup.blocksRaycasts = true;
  }
  
  public void FadeIn(bool resetAlpha = false) {
    // print("fadein called");
    _canvasGroup.interactable = true;
    _canvasGroup.blocksRaycasts = true;
    if (_fadeCoroutine != null) {
      StopCoroutine(_fadeCoroutine);
      _fadeCoroutine = null;
    }
    FadingIn = true;
    FadingOut = false;
    if (resetAlpha) {
      _fadeCoroutine = StartCoroutine(FadeElementInOutCoroutine(0, 1, _inDuration));
    } else {
      _fadeCoroutine = StartCoroutine(FadeElementInOutCoroutine(_canvasGroup.alpha, 1, _inDuration));
    }
  }
  public void FadeOut(bool resetAlpha = false) {
    _canvasGroup.interactable = false;
    _canvasGroup.blocksRaycasts = false;
    if (_fadeCoroutine != null) {
      StopCoroutine(_fadeCoroutine);
      _fadeCoroutine = null;
    }
    FadingOut = true;
    FadingIn = false;
    if (resetAlpha) {
      _fadeCoroutine = StartCoroutine(FadeElementInOutCoroutine(1, 0, _outDuration));
    } else {
      _fadeCoroutine = StartCoroutine(FadeElementInOutCoroutine(_canvasGroup.alpha, 0, _outDuration));
    }
  }
  private IEnumerator FadeElementInOutCoroutine(float startAlpha, float targetAlpha, float duration) {
    float time = 0;
    while (time < duration) {
      time += Time.unscaledDeltaTime;
      _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, _easingCurve.Evaluate(time / duration));
      yield return null;
    }
    _canvasGroup.alpha = targetAlpha;
    _OnFadeComplete?.Invoke();
    FadingIn = false;
    FadingOut = false;
  }
}
