using UnityEngine;
using System.Collections;

public class ParticleLargener : MonoBehaviour {

    public ParticleSystem particles;
    public float scale, min;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        particles.startSize = min + (scale * rigidbody.velocity.magnitude / 300f);

	}
}
