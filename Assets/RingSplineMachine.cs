using UnityEngine;
using System.Collections;

public class RingSplineMachine : MonoBehaviour {

    public int splines;
    public int nodesPerSpline;
    public float meanDistanceBetweenNode;
    public float deviation;
    public Collider spawnVolume;
    public SplineSpawner spawner;

	// Use this for initialization
	void Start () {

        for (int i = 0; i < splines; i++)
        {
            Vector3[] nodes = new Vector3[nodesPerSpline];

            Vector3 start = getPointInBounds(spawnVolume.bounds);
            nodes[0] = start;
            nodes[1] = start + Random.insideUnitSphere * meanDistanceBetweenNode;

            for (int j = 2; j < nodesPerSpline; j++)
            {
                var direction = nodes[j-1] - nodes[j-2];
                direction.Normalize();
                var offset = Random.onUnitSphere * meanDistanceBetweenNode;
                offset = Vector3.RotateTowards(direction, offset, Mathf.PI/2 , 500) ;
                //offset.

                nodes[j] = nodes[j - 1] + offset;
                
            }
            var spline = new GoSpline(nodes);
            spawner.spawnFromSpline(spline);
        }
	}

    Vector3 getPointInBounds(Bounds b)
    {
        return b.center + new Vector3(Random.Range(-1f, 1f) * b.extents.x, Random.Range(-1f, 1f) * b.extents.y, Random.Range(-1f, 1f) * b.extents.z);
    }


	// Update is called once per frame
	void Update () {
	
	}
}
