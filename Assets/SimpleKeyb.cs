using UnityEngine;
using System.Collections;

public class SimpleKeyb : MonoBehaviour {
	public float speed = 1.0f;
	public float rotate_speed = 2.0f;
	
	public SteamVR_Camera steamcam;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float frame_speed = speed * Time.deltaTime;
		float frame_rotate_speed = speed * Time.deltaTime;

		Vector3 curpos = transform.position;
		Quaternion currot = transform.rotation;

		if (Input.GetKey ("w")) { curpos.z += frame_speed; }
		if (Input.GetKey ("s")) { curpos.z -= frame_speed; }
		if (Input.GetKey ("a")) { curpos.x -= frame_speed; }
		if (Input.GetKey ("d")) { curpos.x += frame_speed; }
		if (Input.GetKey ("q")) { curpos.y += frame_speed; }
		if (Input.GetKey ("e")) { curpos.y -= frame_speed; }

		// Rotations
		Quaternion yaw = Quaternion.Euler(0, frame_rotate_speed, 0);
		Quaternion yawneg = Quaternion.Euler(0, -frame_rotate_speed, 0);
		Quaternion pitch = Quaternion.Euler(frame_rotate_speed, 0, 0);
		Quaternion pitchneg = Quaternion.Euler(-frame_rotate_speed, 0, 0);

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
