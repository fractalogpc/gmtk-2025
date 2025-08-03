using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class PitManager : MonoBehaviour
{
    [Header("Asset References")]
    public GameObject fallingPrefab;

    [Header("References")]
    public GenericInteractable offerTrigger;
    public GameManager gameManager;
    public GameObject fallingObject;
    public GameObject rimObject;

    public StudioEventEmitter playerEatenSoundEmitter;
    public StudioEventEmitter crashSoundEmitter;
    public StudioEventEmitter eatSoundEmitter;
    public StudioEventEmitter shrineOfferSoundEmitter;
    public StudioEventEmitter shrineErrorSoundEmitter;
    public ParticleSystem woolExplosion;

    [Header("Settings")]
    public Vector3 pitCenter;
    public Vector3 pitSize;

    public bool IsOfferable {
        get;
        private set;
    }

    public void SetOfferable(bool value) {
        IsOfferable = value;
    }
    
    private void Start() {
        gameManager = GameManager.Instance;
        offerTrigger.OnInteract.AddListener(HandleOffer);
        ResetFallingRocks();
    }

    private void OnDestroy() {
        offerTrigger.OnInteract.RemoveListener(HandleOffer);
    }

    private Coroutine handleOfferCoroutine;
    public void HandleOffer() {
        if (handleOfferCoroutine != null) return;
        if (IsOfferable) {
            shrineOfferSoundEmitter.Play();
        }
        else {
            shrineErrorSoundEmitter.Play();
            return;
        }
        handleOfferCoroutine = StartCoroutine(HandleOfferCoroutine());
        gameManager.SetPlayerMovement(false);
    }
    private IEnumerator HandleOfferCoroutine() {
        MakeFallingFall();
        yield return new WaitForSeconds(3f);
        int sheepCount = CountSheep();
        EatEffects(sheepCount);
        yield return new WaitForSeconds(5f);
        gameManager.AddToQuota(CountSheep());
        ClearPit();
        yield return new WaitForSeconds(6f);
        ResetFallingRocks();
        handleOfferCoroutine = null;
    }

    private void ResetFallingRocks() {
        Vector3 pos = fallingObject.transform.position;
        Quaternion rot = fallingObject.transform.rotation;
        Destroy(fallingObject);
        fallingObject = Instantiate(fallingPrefab, pos, rot, gameObject.transform);
    }

    private Collider[] GetCollidersInPit(string[] tagFilter) {
        List<Collider> colliders = Physics.OverlapBox(transform.position + pitCenter, pitSize / 2).ToList();
        for (int i = colliders.Count - 1; i >= 0; i--) {
           if (!tagFilter.Contains(colliders[i].gameObject.tag)) colliders.RemoveAt(i); 
        }
        return colliders.ToArray();
    }
    
    private int CountSheep() {
        return GetCollidersInPit(new[] { "Sheep" }).Length;
    }

    private void MakeFallingFall() {
        fallingObject.GetComponent<MakeChildrenRigidbodies>().MakeRigidbodies();
        crashSoundEmitter.Play();
    }

    private void EatEffects(int sheepCount = 0) {
        if (woolExplosion != null && sheepCount > 0) woolExplosion.Play();
        if (eatSoundEmitter != null) eatSoundEmitter.Play();
        // camera shake for duration of eatSound
    }

    private void ClearPit() {
       Collider[] colliders = GetCollidersInPit(new[] { "Sheep", "Falling", "Cart" });
       for (int i = colliders.Length - 1; i >= 0; i--) {
          Destroy(colliders[i].gameObject); 
       }
    }

    public void EatPlayer()
    {
        StartCoroutine(EatPlayerCoroutine());
    }

    private IEnumerator EatPlayerCoroutine()
    {
        rimObject.GetComponent<MakeChildrenRigidbodies>().MakeRigidbodies();
        gameManager.SetPlayerMovement(false);
        yield return new WaitForSeconds(1f);
        playerEatenSoundEmitter.Play();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + pitCenter, 0.1f);
        Gizmos.DrawCube(transform.position + pitCenter, pitSize);
    }
}
