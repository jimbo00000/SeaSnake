using UnityEngine;
using System.Collections;

public class HeadOrientation : MonoBehaviour {

	public float magnitude;
	public float frequency;

	// Use this for initialization
	void Start () {
		transform.position = new Vector3(0.0f, 0.0f, 0.0f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(magnitude * Mathf.Sin (frequency * Time.realtimeSinceStartup),
		                                 0.3f * magnitude * Mathf.Sin (2.0f * frequency * (Time.realtimeSinceStartup+0.8f)),
		                                 0.0f);
	}
}
