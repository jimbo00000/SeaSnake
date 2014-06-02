using UnityEngine;

public class AnimatedProjector : MonoBehaviour
{
    public float fps = 25.0f;
    public Texture2D[] frames;

    private int frameIndex;
    private Projector projector;
    public GameObject follow;

    void Start()
    {

        for (int i = 0; i < 80; i++)
        {
            frames[i] = Resources.Load("CausticsRender_0" + ((i+1) < 10 ? "0" + (i+1).ToString() : (i+1).ToString() ) ) as Texture2D;
        }

        projector = GetComponent<Projector>();
        NextFrame();
        InvokeRepeating("NextFrame", 1 / fps, 1 / fps);
    }


    void Update()
    {
        //transform.position = follow.transform.position + Vector3.up * 100;
    }

    void NextFrame()
    {
        projector.material.SetTexture("_ShadowTex", frames[frameIndex]);
        frameIndex = (frameIndex + 1) % frames.Length;
    }
}