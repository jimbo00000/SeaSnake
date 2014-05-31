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

		Quaternion currot = transform.rotation;

        if (Input.GetKey("w")) { transform.position += steamcam.offset.forward * frame_speed; }
        if (Input.GetKey("s")) { transform.position -= steamcam.offset.forward * frame_speed; }
        if (Input.GetKey("a")) { transform.position -= steamcam.offset.right * frame_speed; }
        if (Input.GetKey("d")) { transform.position += steamcam.offset.right * frame_speed;  }
        if (Input.GetKey("q")) { transform.position += steamcam.offset.up * frame_speed; }
        if (Input.GetKey("e")) { transform.position -= steamcam.offset.up * frame_speed; }

		// Rotations
		Quaternion yaw = Quaternion.Euler(0, frame_rotate_speed, 0);
		Quaternion yawneg = Quaternion.Euler(0, -frame_rotate_speed, 0);
		Quaternion pitch = Quaternion.Euler(frame_rotate_speed, 0, 0);
		Quaternion pitchneg = Quaternion.Euler(-frame_rotate_speed, 0, 0);

		if (Input.GetKey ("j")) { currot *= yaw; }
		if (Input.GetKey ("l")) { currot *= yawneg; }
		if (Input.GetKey ("i")) { currot *= pitch; }
		if (Input.GetKey ("k")) { currot *= pitchneg; }

		
		transform.rotation = currot;

	}
}
