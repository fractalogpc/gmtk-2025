using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;

public class PitManager : MonoBehaviour
{
    [Header("Asset References")]
    public GameObject fallingPrefab;

    [Header("References")]
    public GenericInteractable offerTrigger;
    public GameManager gameManager;
    public GameObject fallingObject;
    public StudioEventEmitter cthutluSoundsEmitter;
    public StudioEventEmitter eatSoundEmitter;
    public ParticleSystem woolExplosion;

    [Header("Settings")]
    public Vector3 pitCenter;
    public Vector3 pitSize;

    private void Start() {
        Debug.LogWarning("Matthew I disabled pit manager because there were errors");
        this.enabled = false;
        return;

        offerTrigger.OnInteract.AddListener(HandleOffer);
        ResetFallingRocks();
    }

    private void OnDestroy() {
        offerTrigger.OnInteract.RemoveListener(HandleOffer);
    }

    private Coroutine handleOfferCoroutine;
    public void HandleOffer() {
        handleOfferCoroutine = StartCoroutine(HandleOfferCoroutine());
    }

    private IEnumerator HandleOfferCoroutine() {
       MakeFallingFall();
       yield return new WaitForSeconds(3f);
       CountSheep();
       EatEffects();
    }

    private void ResetFallingRocks() {
        Vector3 pos = fallingObject.transform.position;
        Quaternion rot = fallingObject.transform.rotation;
        Destroy(fallingObject);
        fallingObject = Instantiate(fallingPrefab, pos, rot, gameObject.transform);
    }

    private Collider[] GetCollidersInPit(string[] tagFilter) {
        List<Collider> colliders = Physics.OverlapBox(pitCenter, pitSize / 2).ToList();
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
    }

    private void EatEffects() {
        woolExplosion.Play();
        eatSoundEmitter.Play();
        // camera shake for duration of eatSound
    }

    private void ClearPit() {
       Collider[] colliders = GetCollidersInPit(new[] { "Sheep", "Falling", "Cart" });
       for (int i = colliders.Length; i > 0; i++) {
          Destroy(colliders[i].gameObject); 
       }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pitCenter, 0.1f);
        Gizmos.DrawWireCube(pitCenter, pitSize);
    }
}
