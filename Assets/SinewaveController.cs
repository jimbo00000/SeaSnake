using UnityEngine;
using System.Collections;

public class SinewaveController : MonoBehaviour {

    public float myAmplitude;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.one *Mathf.Sin(Time.timeSinceLevelLoad) * myAmplitude;
	}
}
