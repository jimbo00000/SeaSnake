using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class SplineSpawner : MonoBehaviour {

    public float spawnRadius;

    public bool RandomRotationX, RandomRotationY, RandomRotationZ;
    public bool equidistant;
    public bool orientAlongSpline;
    public int minAmount, maxAmount;
    public bool checkForCollision;
    public float collisionRadius;
    public LayerMask collisionLayermask;

    public GameObject itemPrefab;
    public List<GameObject> items = new List<GameObject>();

    private GoDummyPath pathEditor;
    private GoSpline spline;

    private const float epsilon = .001f;

	// Use this for initialization
	void Start () {
        pathEditor = GetComponent<GoDummyPath>();
	}


    public void spawnFromSpline(GoSpline spline)
    {    
        spline.buildPath();
        int amount = Random.Range(minAmount, maxAmount);

        for (int i = 0; i < amount; i++)
        {
            float t = equidistant ? (float)i / amount : Random.value;

            Vector3 initialPosition = spline.getPointOnPath(t);
            initialPosition += Random.insideUnitSphere * spawnRadius;
            if (!checkForCollision || !Physics.CheckSphere(initialPosition, collisionRadius, collisionLayermask))
            {
                var item = Instantiate(itemPrefab, initialPosition, getRandomRotation()) as GameObject;
                if (orientAlongSpline)
                {
                    item.transform.forward = spline.getPointOnPath(t + epsilon) - spline.getPointOnPath(t);
                }

                item.transform.parent = transform.parent;
                items.Add(item);

            }
        }
    }


    public void spawn()
    {
        if (!pathEditor.localSpace)
            spline = new GoSpline(pathEditor.nodes);
        else
            spline = new GoSpline(pathEditor.nodes.Select(n => transform.TransformPoint( n)).ToArray());
        spline.buildPath();
        int amount = Random.Range(minAmount,maxAmount);

        for (int i = 0; i < amount ; i++)
        {
            float t = equidistant ? (float)i /amount  : Random.value;

            Vector3 initialPosition = spline.getPointOnPath(t);
            initialPosition += Random.insideUnitSphere * spawnRadius;
            if (!checkForCollision || !Physics.CheckSphere(initialPosition, collisionRadius, collisionLayermask))
            {
                var item = Instantiate(itemPrefab, initialPosition, getRandomRotation()) as GameObject;
                if (orientAlongSpline)
                {
                    item.transform.forward = spline.getPointOnPath(t + epsilon) - spline.getPointOnPath(t);
                }

                item.transform.parent = transform.parent;
                items.Add(item);
            
            }
        }
    }

    Quaternion getRandomRotation()
    {
        return Quaternion.Euler(RandomRotationX ? Random.value * 360 : 0, RandomRotationY ? Random.value * 360 : 0, RandomRotationZ ? Random.value * 360 : 0);
    }

}

