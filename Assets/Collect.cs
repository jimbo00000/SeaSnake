using UnityEngine;
using System.Collections;

public class Collect : MonoBehaviour {

    public AudioClip chime;
    bool collected = false;
    public static int score = 0;

    void OnTriggerEnter()
    {
        if (!collected)
        {
            audio.PlayOneShot(chime);
            Destroy(gameObject, 1);
            collected = true;
            score++;
        }
     
    }
    
}
