using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomRotation : MonoBehaviour
{
	public float speedMin, speedMax;
	private void Update() {
		transform.Rotate(Vector3.up * Time.deltaTime * Random.Range(speedMin, speedMax));	
		transform.Rotate(Vector3.right * Time.deltaTime * Random.Range(speedMin, speedMax));	
		transform.Rotate(Vector3.forward * Time.deltaTime * Random.Range(speedMin, speedMax));	
	}
}
