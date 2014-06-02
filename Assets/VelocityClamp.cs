using UnityEngine;
using System.Collections;

public class VelocityClamp : MonoBehaviour {

    public float maxVelocity;
	
	// Update is called once per frame
	void Update () {
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, maxVelocity);   
	}
}
