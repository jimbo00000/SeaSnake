using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour {

    public GameObject terrain;
    public GameObject tail;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(transform.position, terrain.transform.position) > 4500)
        {
            
            Debug.Log("teleport");
            transform.Rotate(0, 180, 0);
            rigidbody.velocity = -rigidbody.velocity;
        }
	}
}
