using UnityEngine;
using System.Collections;

public class RaycastSpawner : MonoBehaviour {

    public int amount;
    public GameObject[] things;
    public float minScaling, maxScaling;
    public bool doit = false;

	// Use this for initialization
	void Start () {
        spawn();
    }


    void spawn()
    {
        for (int i = 0; i < amount; i++)
        {
            var point = pointOnSurface();
            RaycastHit info;
            if (Physics.Raycast(point, Vector3.down, out info))
            {
                var thing = Instantiate(things[Random.Range(0, things.Length)], info.point , Quaternion.identity) as GameObject;
                thing.transform.up = info.normal;
                thing.transform.localScale = Vector3.one * Random.Range(minScaling, maxScaling);
            }
        }
    }

    Vector3 pointOnSurface()
    {
        return collider.bounds.center + new Vector3(Random.Range(-1f, 1f) * collider.bounds.extents.x, 0, Random.Range(-1f, 1f) * collider.bounds.extents.z);
    }

	// Update is called once per frame
	void Update () {
        if (doit)
        {
            doit = false;
            spawn();
        }
	}
}
