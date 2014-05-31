using UnityEngine;
using System.Collections;

public class PerlinTerrain : MonoBehaviour
{

    //Toolkit instance.
    [SerializeField]
    TerrainToolkit kit;
    //Array of textures.
    [SerializeField]
    Texture2D sandTexture;
    [SerializeField]
    Texture2D grassTexture;
    [SerializeField]
    Texture2D rockTexture;
    [SerializeField]
    Texture2D cliffTexture;

    void Start()
    {
        //Check if we have the kit assigned;.
        if (kit == null)
        {
            //If there is not an instance assigned or on gameObject, return.
            if (!GetComponent<TerrainToolkit>())
            {
                return;
            }
            //Else assign kit to gameObject's toolkit.
            else
            {
                kit = GetComponent<TerrainToolkit>();
            }
        }
        //Generate the terrain.
        Generate();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    public void Generate()
    {
        //Generate the perlin terrain.
        kit.PerlinGenerator((int)Random.Range(3, 6), Random.Range(0.4f, 0.9f), Random.Range(2, 6), 1f);
        //Gives it a less smooth feel.
        kit.PerlinGenerator(4, 4, 4, 0.1f);
        //Creates arrays for stops.
        float[] slopeStops = new float[2];
        float[] heightStops = new float[4];
        Texture2D[] terrainTextures = new Texture2D[4];
        //Assigns values to the arrays.
        slopeStops[0] = 30f; slopeStops[1] = 70f;
        heightStops[0] = Random.Range(0.05f, 0.18f);
        heightStops[1] = Random.Range(0.19f, 0.49f);
        heightStops[2] = Random.Range(0.5f, 0.69f);
        heightStops[3] = Random.Range(0.7f, 0.89f);
        terrainTextures[0] = cliffTexture;
        terrainTextures[1] = sandTexture;
        terrainTextures[2] = grassTexture;
        terrainTextures[3] = rockTexture;
        //Paints the textures.
        kit.TextureTerrain(slopeStops, heightStops, terrainTextures);
    }

    public void Generate(bool doublePerlin)
    {
        //Generate the perlin terrain.
        kit.PerlinGenerator((int)Random.Range(3, 6), Random.Range(0.4f, 0.9f), Random.Range(2, 6), 1f);
        //Gives it a less smooth feel.
        if (!doublePerlin)
        {
            kit.PerlinGenerator(4, 4, 4, 0.1f);
        }
        //Creates arrays for stops.
        float[] slopeStops = new float[2];
        float[] heightStops = new float[4];
        Texture2D[] terrainTextures = new Texture2D[4];
        //Assigns values to the arrays.
        slopeStops[0] = 30f; slopeStops[1] = 70f;
        heightStops[0] = Random.Range(0.05f, 0.18f);
        heightStops[1] = Random.Range(0.19f, 0.49f);
        heightStops[2] = Random.Range(0.5f, 0.69f);
        heightStops[3] = Random.Range(0.7f, 0.89f);
        terrainTextures[0] = cliffTexture;
        terrainTextures[1] = sandTexture;
        terrainTextures[2] = grassTexture;
        terrainTextures[3] = rockTexture;
        //Paints the textures.
        kit.TextureTerrain(slopeStops, heightStops, terrainTextures);
    }

}