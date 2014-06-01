using UnityEngine;
using System.Collections;

public class DisplacementIndicator : MonoBehaviour {

    public float maximumDisplacement;
    public LineRenderer line;
    public GameObject head;
    public GameObject tail;
    public float screenDistance;
    public SteamVR_Camera steamCam;
    public int lineVerts;

	// Use this for initialization
	void Start () {
        line.SetVertexCount(lineVerts);
	}
	
	// Update is called once per frame
	void Update () {
	    //get displacement vector
        var localTail = head.transform.InverseTransformPoint(tail.transform.position);
        Vector2 displacement = new Vector2(localTail.x, localTail.y);

        Vector3 cameraFacePosition = steamCam.offset.position + steamCam.offset.forward * screenDistance;

        Vector3 linePosition = cameraFacePosition + (steamCam.offset.right * displacement.x ) + (steamCam.offset.up * displacement.y );

        

        //render line
        for (int i = 0; i < lineVerts; i++)
			{

                var position = Vector3.Lerp(linePosition,cameraFacePosition, (float)i/lineVerts);

                line.SetPosition(i, position/maximumDisplacement);
	 
			}

        
	}
}
