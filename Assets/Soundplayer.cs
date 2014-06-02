using UnityEngine;
using System.Collections;

public class Soundplayer : MonoBehaviour {
    public AudioClip bubbles;
    public AudioClip sploosh;
    public AudioSource player;
    public AudioSource constantPlayer;

    bool played = false;
    bool played2 = false;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        if (!played && rigidbody.velocity.magnitude > 100)
        {
            played = true;
            player.PlayOneShot(bubbles);
        }
        else if (rigidbody.velocity.magnitude < 100)
        {
            played = false;
        }

        if (!played2 && rigidbody.velocity.magnitude > 299)
        {
            played2 = true;
            player.PlayOneShot(sploosh);
        }
        else if (rigidbody.velocity.magnitude < 299)
        {
            played2 = false;
        }

        constantPlayer.volume = rigidbody.velocity.magnitude / 300;

	}
}
