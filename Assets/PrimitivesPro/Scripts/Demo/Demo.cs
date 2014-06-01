using System.Collections.Generic;
using PrimitivesPro.GameObjects;
using PrimitivesPro.Primitives;
using UnityEngine;
using Plane = PrimitivesPro.GameObjects.PlaneObject;

internal class Demo : MonoBehaviour
{
    /// <summary>
    /// instance of this class
    /// </summary>
    public static Demo Instance
    {
        get { return instance; }
    }

    private static Demo instance;

    public float animTimeMax = 5.0f;

    private List<GameObject> buttons;

    private BaseObject shapeMain;
    private BaseObject shapeOld;
    private float animTimeout;
    private float[] shapeParamsStart;
    private float[] shapeParamsMax;
    private int shapeID;

//    private Vector3 centralPosition = new Vector3(4, 0.1f, 3);
//    private Vector3 prevPosition = new Vector3(-8, 0.1f, 3);
//    private Vector3 nextPosition = new Vector3(16, 0.1f, 3);

    public Transform centralPosition = null;
    public Transform prevPosition = null;
    public Transform nextPosition = null;

    private float nextShowTimeout;
    private float nextShowTimeoutMax = 1;

    private bool textureToggle;
    private bool flatNormals;

    private void Start()
    {
        instance = this;
        buttons = new List<GameObject>(16);

        animTimeout = 0.0f;
        shapeParamsStart = new float[6];
        shapeParamsMax = new float[6];

        Vector3 pos = Vector3.zero;
        pos.z = -0.8f;
        pos.x = -2.0f;

        for (int i = 0; i < 16; i++)
        {
            CreateButton(i, pos);
            pos.x = pos.x + 0.8f;
        }

        CreateButton(16, new Vector3(10, 0, 5));
        CreateButton(17, new Vector3(10, 0, 3.8f));

        buttons[16].renderer.material = new Material(Shader.Find("Unlit/Texture"));
        buttons[16].renderer.material.mainTexture = Resources.Load("sphereChecker") as Texture2D;

        buttons[17].renderer.material = new Material(Shader.Find("Unlit/Texture"));
        buttons[17].renderer.material.mainTexture = Resources.Load("faceNormalsIcon") as Texture2D;

        OnButtonHit(0);
    }

    /// <summary>
    /// event when user pressed the button
    /// </summary>
    /// <param name="id">id of the button</param>
    public void OnButtonHit(int id)
    {
        animTimeout = animTimeMax;

        // create buttons shapes
        BaseObject shapeObject = null;

        switch (id)
        {
            case 0:
                shapeObject = Plane.Create(1f, 1f, 1, 1);
                shapeParamsMax = new float[] {4.0f, 4.0f, 1, 1, 0, 0};
                shapeParamsStart = new float[] {1f, 1f, 1, 1, 0, 0};
                break;

            case 1:
                shapeObject = Circle.Create(1.0f, 3);
                shapeParamsMax = new float[] {2.5f, 40, 0, 0, 0, 0};
                shapeParamsStart = new float[] {1.0f, 3, 0, 0, 0, 0};
                break;

            case 2:
                shapeObject = Ellipse.Create(1.0f, 0.5f, 3);
                shapeParamsMax = new float[] {2.5f, 1.2f, 40, 0, 0, 0};
                shapeParamsStart = new float[] {1.0f, 0.5f, 3, 0, 0, 0};
                break;

            case 3:
                shapeObject = Ring.Create(0.5f, 1.0f, 3);
                shapeParamsMax = new float[] {1.0f, 2.5f, 40, 0, 0, 0};
                shapeParamsStart = new float[] {0.5f, 1.0f, 3, 0, 0, 0};
                break;

            case 4:
                shapeObject = Box.Create(1f, 1f, 1f, 1, 1, 1, false, null, PivotPosition.Botttom);
                shapeParamsMax = new float[] {2.5f, 2.5f, 2.5f, 1, 1, 1};
                shapeParamsStart = new float[] {1f, 1f, 1f, 1, 1, 1};
                break;

            case 5:
                shapeObject = Cylinder.Create(1f, 3, 3, 1,  flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                              PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.25f, 4f, 40, 1, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 3, 3, 1, 0, 0, 0};
                break;

            case 6:
                shapeObject = Cone.Create(1, 0, 0, 2, 3, 10, flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                          PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.25f, 0, 4f, 40, 10, 0, 0};
                shapeParamsStart = new float[] {1, 1, 2, 3, 10, 0, 0};
                break;

            case 7:
                shapeObject = Sphere.Create(1f, 4, 0, 0, flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                            PivotPosition.Botttom);
                shapeParamsMax = new float[] {2.25f, 40, 0, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 4, 0, 0, 0, 0};
                break;

            case 8:
                shapeObject = Ellipsoid.Create(1, 1, 1, 4, flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                               PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.25f, 2.45f, 2.5f, 40, 0, 0, 0};
                shapeParamsStart = new float[] {1, 1, 1, 4, 0, 0, 0};
                break;

            case 9:
                shapeObject = Pyramid.Create(1, 1, 1, 1, 1, 1, false, PivotPosition.Botttom);
                shapeParamsMax = new float[] {2.7f, 2.7f, 1.7f, 1, 1, 1, 0, 0, 0};
                shapeParamsStart = new float[] {1, 1, 1, 1, 1, 1, 0, 0, 0};
                break;

            case 10:
                shapeObject = GeoSphere.Create(1f, 0, PrimitivesPro.Primitives.GeoSpherePrimitive.BaseType.Icosahedron,
                                               flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                               PivotPosition.Botttom);
                shapeParamsMax = new float[] {2.45f, 4, 0, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 0, 0, 0, 0, 0};
                break;

            case 11:
                shapeObject = Tube.Create(0.8f, 1f, 1f, 3, 1, 0.0f, false, flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                          PivotPosition.Botttom);
                shapeParamsMax = new float[] {0.8f, 1.5f, 4f, 40, 0, 0, 0, 0};
                shapeParamsStart = new float[] {0.8f, 1f, 1f, 3, 0, 0, 0, 0};
                break;

            case 12:
                shapeObject = Capsule.Create(1f, 1f, 4, 1, false, flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                             PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.2f, 4f, 40, 1, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 1f, 4, 1, 0, 0, 0};
                break;

            case 13:
                shapeObject = RoundedCube.Create(1f, 1f, 1f, 1, 0.2f,
                                                 flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                 PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.6f, 1.6f, 1.6f, 20, 0.6f, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 1f, 1f, 1, 0.2f, 0, 0, 0};
                break;

            case 14:
                shapeObject = Torus.Create(1f, 0.5f, 4, 4, 0, flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                           PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.6f, 0.8f, 40, 40, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 0.5f, 4, 4, 0, 0, 0};
                break;

            case 15:
                shapeObject = TorusKnot.Create(0.5f, 0.3f, 10, 4, 2, 3,
                                               flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                               PivotPosition.Botttom);
                shapeParamsMax = new float[] {1f, 0.5f, 120, 40, 2, 3, 0, 0, 0};
                shapeParamsStart = new float[] {0.5f, 0.3f, 10, 4, 2, 3, 0, 0, 0};
                break;

            case 16:
                textureToggle = !textureToggle;
                break;

            case 17:
                flatNormals = !flatNormals;
                break;
        }

        if (shapeObject)
        {
            if (shapeOld)
            {
                Destroy(shapeOld.gameObject);
            }

            shapeOld = shapeMain;
            shapeMain = shapeObject;

            shapeMain.gameObject.renderer.material = new Material(GetSpecularShader());
            shapeMain.gameObject.renderer.material.SetColor("_Color", new Color(1.0f, 180.0f/255f, 180f/255f));
            shapeMain.gameObject.renderer.material.SetColor("_SpecColor", Color.white);
            shapeMain.gameObject.transform.position = prevPosition.position;

            nextShowTimeout = nextShowTimeoutMax;

            shapeID = id;
        }

        if (textureToggle)
        {
            shapeMain.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Checker") as Material;
        }
        else
        {
            shapeMain.GetComponent<MeshRenderer>().sharedMaterial = new Material(GetSpecularShader());
            shapeMain.gameObject.renderer.material.SetColor("_Color", new Color(1.0f, 180.0f/255f, 180f/255f));
            shapeMain.gameObject.renderer.material.SetColor("_SpecColor", Color.white);
        }
    }

    Shader GetSpecularShader()
    {
        return Shader.Find("Specular");
    }

    /// <summary>
    /// event when user hover over the button
    /// </summary>
    /// <param name="id"></param>
    public void OnButtonHover(int id, bool start)
    {
        var transform = buttons[id].transform;
        var material = buttons[id].GetComponent<MeshRenderer>().sharedMaterial;

        if (start)
        {
            material.color = new Color(1, 0, 0);
            transform.position -= new Vector3(0, 0.28f, 0.0f);
        }
        else
        {
            material.color = new Color(1, 1, 1);
            transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
        }
    }

    /// <summary>
    /// create plane button
    /// </summary>
    /// <param name="id">id of the button</param>
    /// <param name="position"></param>
    private void CreateButton(int id, Vector3 position)
    {
        var plane = Plane.Create(0.8f, 0.8f, 2, 2);
        plane.gameObject.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1, 1, 1);

        var collider = plane.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        plane.gameObject.transform.position = position;

        var trigger = plane.gameObject.AddComponent<ButtonTrigger>();
        trigger.ID = id;

        buttons.Add(plane.gameObject);

        switch (id)
        {
            case 0:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("plane") as Texture2D;
                break;

            case 1:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("circle") as Texture2D;
                break;

            case 2:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("ellipse") as Texture2D;
                break;

            case 3:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("ring") as Texture2D;
                break;

            case 4:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("box") as Texture2D;
                break;

            case 5:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("cylinder") as Texture2D;
                break;

            case 6:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("cone") as Texture2D;
                break;

            case 7:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("sphere") as Texture2D;
                break;

            case 8:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("ellipsoid") as Texture2D;
                break;

            case 9:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("pyramid") as Texture2D;
                break;

            case 10:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("geosphere") as Texture2D;
                break;

            case 11:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("tube") as Texture2D;
                break;

            case 12:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("capsule") as Texture2D;
                break;

            case 13:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("roundedCube") as Texture2D;
                break;

            case 14:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("torus") as Texture2D;
                break;

            case 15:
                plane.gameObject.renderer.material = new Material(Shader.Find("Unlit/Texture"));
                plane.gameObject.renderer.material.mainTexture = Resources.Load("torusKnot") as Texture2D;
                break;
        }

        if (plane)
        {
            plane.gameObject.transform.position = position + new Vector3(0, 0.1f, 0);
        }
    }

    private void Update()
    {
        if (nextShowTimeout > 0)
        {
            var t = 1.0f - nextShowTimeout/nextShowTimeoutMax;

            if (shapeOld)
            {
                var newPos = Vector3.Lerp(shapeOld.gameObject.transform.position, nextPosition.position, t);
                shapeOld.gameObject.transform.position = newPos;
            }

            if (shapeMain)
            {
                var newPos = Vector3.Lerp(shapeMain.gameObject.transform.position, centralPosition.position, t);
                shapeMain.gameObject.transform.position = newPos;
            }

            nextShowTimeout -= Time.deltaTime;

            if (shapeOld != null && nextShowTimeout <= 0)
            {
                Object.Destroy(shapeOld.gameObject);
                shapeOld = null;
            }
        }

        if (shapeMain != null)
        {
            animTimeout -= Time.deltaTime;

            shapeMain.gameObject.transform.rotation = Quaternion.Euler(40, 0, 0);

            var rotation = Quaternion.Euler(0, 360*animTimeout/animTimeMax, 0);

            shapeMain.gameObject.transform.rotation *= rotation;

            if (animTimeout > 0)
            {
                var shapeParams = new float[6];

                float t = 1.0f - animTimeout/animTimeMax;

                for (int i = 0; i < 6; i++)
                {
                    shapeParams[i] = shapeParamsStart[i]*(1 - t) + shapeParamsMax[i]*t;
                }

                switch (shapeID)
                {
                    case 0:
                        ((Plane) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], 1, 1);
                        break;

                    case 1:
                        ((Circle) shapeMain).GenerateGeometry(shapeParams[0], (int) shapeParams[1]);
                        break;

                    case 2:
                        ((Ellipse) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int) shapeParams[2]);
                        break;

                    case 3:
                        ((Ring) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int) shapeParams[2]);
                        break;

                    case 4:
                        ((Box) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2], 1, 1, 1,
                                                           false, null, PivotPosition.Center);
                        break;

                    case 5:
                        ((Cylinder) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int) shapeParams[2], 1,
                                                                flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                                PivotPosition.Center);
                        break;

                    case 6:
                        ((Cone) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], 0, shapeParams[2],
                                                            (int) shapeParams[3], (int) shapeParams[4],
                                                            flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                            PivotPosition.Center);
                        break;

                    case 7:
                        ((Sphere) shapeMain).GenerateGeometry(shapeParams[0], (int) shapeParams[1], 0.0f, 0.0f,
                                                              flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                              PivotPosition.Center);
                        break;

                    case 8:
                        ((Ellipsoid) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2],
                                                                 (int) shapeParams[3],
                                                                 flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                                 PivotPosition.Center);
                        break;

                    case 9:
                        ((Pyramid) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2], 1, 1, 1,
                                                               false, PivotPosition.Center);
                        break;

                    case 10:
                        ((GeoSphere) shapeMain).GenerateGeometry(shapeParams[0], (int) shapeParams[1],
                                                                 PrimitivesPro.Primitives.GeoSpherePrimitive.BaseType.
                                                                     Icosahedron,
                                                                 flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                                 PivotPosition.Center);
                        break;

                    case 11:
                        ((Tube) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2],
                                                            (int) shapeParams[3], (int) shapeParams[4],
                                                            (float) shapeParams[4], false,
                                                            flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                            PivotPosition.Center);
                        break;

                    case 12:
                        ((Capsule) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int) shapeParams[2],
                                                               (int) shapeParams[3], false,
                                                               flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                               PivotPosition.Center);
                        break;

                    case 13:
                        ((RoundedCube) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2],
                                                                   (int) shapeParams[3], shapeParams[4],
                                                                   flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                                   PivotPosition.Center);
                        break;

                    case 14:
                        ((Torus) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int) shapeParams[2],
                                                             (int) shapeParams[3], 0,
                                                             flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                             PivotPosition.Center);
                        break;

                    case 15:
                        ((TorusKnot) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int) shapeParams[2],
                                                                 (int) shapeParams[3], 3, 2,
                                                                 flatNormals ? NormalsType.Face : NormalsType.Vertex,
                                                                 PivotPosition.Center);
                        break;
                }
            }
        }
    }

    private void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
    }
}
