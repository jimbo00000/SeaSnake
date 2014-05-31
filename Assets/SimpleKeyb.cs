using UnityEngine;
using System.Collections;

public class SimpleKeyb : MonoBehaviour {
	public float speed = 1.0f;
	public float rotate_speed = 2.0f;
	public SteamVR_Camera steamcam;
	public float velocityDamping = 0.1f;

	Vector3 velocity = new Vector3(0.0f, 0.0f, 0.0f);
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float frame_rotate_speed = speed * Time.deltaTime;

		Quaternion currot = transform.rotation;

		// Rotations
		Quaternion yaw = Quaternion.Euler(0, frame_rotate_speed, 0);
		Quaternion yawneg = Quaternion.Euler(0, -frame_rotate_speed, 0);
		Quaternion pitch = Quaternion.Euler(frame_rotate_speed, 0, 0);
		Quaternion pitchneg = Quaternion.Euler(-frame_rotate_speed, 0, 0);

		if (Input.GetKey ("j")) { currot *= yaw; }
		if (Input.GetKey ("l")) { currot *= yawneg; }
		if (Input.GetKey ("i")) { currot *= pitch; }
		if (Input.GetKey ("k")) { currot *= pitchneg; }

		//transform.rotation = currot;
		//MoveDirect ();
		MoveReflect ();

		velocity *= (1.0f - velocityDamping);
		transform.position += 10.0f * velocity * Time.deltaTime;
	}

	void MoveDirect () {
		float frame_speed = speed * Time.deltaTime;
		
		if (Input.GetKey("w")) { transform.position += steamcam.offset.forward * frame_speed; }
		if (Input.GetKey("s")) { transform.position -= steamcam.offset.forward * frame_speed; }
		if (Input.GetKey("a")) { transform.position -= steamcam.offset.right * frame_speed; }
		if (Input.GetKey("d")) { transform.position += steamcam.offset.right * frame_speed;  }
		if (Input.GetKey("q")) { transform.position += steamcam.offset.up * frame_speed; }
		if (Input.GetKey("e")) { transform.position -= steamcam.offset.up * frame_speed; }
	}

	void MoveReflect () {
		float frame_speed = speed * Time.deltaTime;

		// Take the reflection of the head motion vector about the normal of a fin
		// (right vector) for a motive force
		Transform headpose = steamcam.offset;

		Vector3 headVelocity = new Vector3 (0.0f, 0.0f, 0.0f);
		if (Input.GetKey ("a")) { headVelocity.x = frame_speed; }
		if (Input.GetKey ("d")) { headVelocity.x = -frame_speed; }

		Vector3 finNormal = headpose.right; ///@todo handle up, down as well
		if (Vector3.Dot (finNormal, headVelocity) < 0.0f) {
			finNormal *= -1.0f;
		}

		Vector3 backForce = -Vector3.Reflect (headVelocity, finNormal);
		//float dotprod = Vector3.Dot (finNormal, headVelocity);
		//transform.position += -backForce * (1.0f - dotprod);
		//transform.position += headpose.right;

		// Strike off lateral components of forward motion
		Vector3 globalForward = new Vector3 (0.0f, 0.0f, -1.0f);
		backForce = Vector3.Project (backForce, globalForward);
		velocity -= 100.0f * frame_speed * backForce;


		// debugging
		Debug.DrawLine(transform.position, transform.position + 10.0f * globalForward, Color.white);
		Debug.DrawLine(transform.position, transform.position + 100.0f * headVelocity, Color.cyan);
		//Debug.DrawLine(transform.position, transform.position + 10.0f * finNormal, Color.yellow);
		Debug.DrawLine(transform.position, transform.position + 100.0f * backForce, Color.magenta);
	}
}
