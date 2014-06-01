using UnityEngine;
using System.Collections;

public class HeadPosition : MonoBehaviour {

	public float magnitude;
	public float frequency;
	public float phase;
	public GameObject target;
    public GameObject tail;
    public GameObject controller;
    public float forceScale;
    public float torqueScale;
    public float buoyancyScale;

	// Use this for initialization
	void Start () {
		//transform.position = new Vector3(0.0f, 0.0f, 2.0f);
	}
	
	// Update is called once per frame
	void Update () {
		/*transform.position = new Vector3(magnitude * Mathf.Sin (frequency * (Time.realtimeSinceStartup+phase)),
		                                 0.2f * magnitude * Mathf.Sin (2.0f * frequency * (Time.realtimeSinceStartup+0.4f)),
		                                 2.0f);
		transform.LookAt (target.transform);
        */
        Vector3 headDirection = controller.GetComponentInChildren<SteamVR_Camera>().offset.transform.forward;
        Vector3 headUp = controller.GetComponentInChildren<SteamVR_Camera>().offset.up;

        Vector3 relativeVelocity = tail.rigidbody.velocity - rigidbody.velocity;
        Vector3 inPlaneRelVel = relativeVelocity - (headDirection * Vector3.Dot(relativeVelocity, headDirection) );


        float forceAmount =  (inPlaneRelVel).sqrMagnitude * forceScale;
        
        var forceVector =  headDirection * forceAmount;
        controller.rigidbody.AddForce(forceVector);

        var torque = Vector3.Cross(controller.rigidbody.velocity.normalized, inPlaneRelVel) * torqueScale;
        
        var uprightTorque = Vector3.Cross(controller.transform.up, Vector3.up) * buoyancyScale;
        controller.rigidbody.AddTorque(torque + uprightTorque);


        //project velocity in direction of head
        float dot = Vector3.Dot(rigidbody.velocity.normalized, headDirection);

        controller.rigidbody.velocity = dot * controller.rigidbody.velocity + ((1 - dot) * controller.rigidbody.velocity.magnitude * headDirection);

	}
}
