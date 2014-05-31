﻿using UnityEngine;
using System.Collections;

public class SimpleKeyb : MonoBehaviour {
	public float speed = 0.2f;
	public float rotate_speed = 2.0f;
	
	public SteamVR_Camera steamcam;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 curpos = transform.position;
		Quaternion currot = transform.rotation;

		if (Input.GetKey ("w")) { curpos.z += speed; }
		if (Input.GetKey ("s")) { curpos.z -= speed; }
		if (Input.GetKey ("a")) { curpos.x -= speed; }
		if (Input.GetKey ("d")) { curpos.x += speed; }
		if (Input.GetKey ("q")) { curpos.y += speed; }
		if (Input.GetKey ("e")) { curpos.y -= speed; }

		// Rotations
		Quaternion yaw = Quaternion.Euler(0, rotate_speed, 0);
		Quaternion yawneg = Quaternion.Euler(0, -rotate_speed, 0);
		Quaternion pitch = Quaternion.Euler(rotate_speed, 0, 0);
		Quaternion pitchneg = Quaternion.Euler(-rotate_speed, 0, 0);

		if (Input.GetKey ("j")) { currot *= yaw; }
		if (Input.GetKey ("l")) { currot *= yawneg; }
		if (Input.GetKey ("i")) { currot *= pitch; }
		if (Input.GetKey ("k")) { currot *= pitchneg; }

		transform.position = curpos;
		transform.rotation = currot;

		// overwrite for now
		transform.rotation = steamcam.offset.rotation;
	}
}
