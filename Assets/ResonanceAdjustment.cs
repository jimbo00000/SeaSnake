using UnityEngine;
using System.Collections;

public class ResonanceAdjustment : MonoBehaviour {

	public GameObject velController;
	public ConfigurableJoint mainJoint;
	public float minJointXYspring;
	public float jointXYspringScale;

	// Use this for initialization
	void Start () {
		//mainJoint.angularYZDrive.positionSpring = minJointXYspring;
	}
	
	// Update is called once per frame
	void Update () {
		float velMag = velController.rigidbody.velocity.magnitude;
		var drive = mainJoint.angularYZDrive;

		drive.positionSpring = minJointXYspring + velMag*jointXYspringScale;
			mainJoint.angularYZDrive = drive;//   .positionSpring = minJointXYspring + velMag*jointXYspringScale;
	}
}
