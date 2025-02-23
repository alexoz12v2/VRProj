using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGen : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public int depth = 2;
    public float scale = 20f;
    public float power = 2;
    public Texture2D maskTexture; // Mask to protect areas
    [HideInInspector, SerializeField]
    public float[,] TerrainHeights;
    
    public void GenerateTerrain()
    {
        TerrainHeights = new float[width, height];
        Terrain terrain = GetComponent<Terrain>();
        
        if (terrain == null || terrain.terrainData == null) return;
        
        
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
    }

    public void StoreTerrainHeights(TerrainData terrainData)
    {
        TerrainHeights = terrainData.GetHeights(0, 0, width, height);

    }

    TerrainData GenerateTerrainData(TerrainData terrainData)
    {

        terrainData.heightmapResolution = width +1;
        terrainData.size = new Vector3(width, depth, height);
        StoreTerrainHeights(terrainData);
        terrainData.SetHeights(0, 0, GenerateHeights(terrainData));
        transform.position = new Vector3(transform.position.x, -terrainData.bounds.extents.y, transform.position.z);
        return terrainData;
    }

    float[,] GenerateHeights(TerrainData terrainData)
    {
        float[,] heights = new float[width, height];
        
        //init to zero heights
        for (int i = 0; i< width; i++)
            for (int j = 0; j < height; j++)
                heights[i, j] = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                // Apply mask if available
                if (maskTexture != null)
                {
                    Color maskColor = maskTexture.GetPixel(x, y);
                    
                    if (maskColor.r > 0.5f)
                    {
                        float noiseValue = 0;
                        for (int scale = 1; scale <= 16; scale <<= 1)
                        {

                            float xCoord = (float)x / width * scale;
                            float yCoord = (float)y / height * scale;
                            noiseValue += 0.5f*Mathf.PerlinNoise(xCoord, yCoord)/scale;

                            
                            if (noiseValue > 1)
                            {
                                noiseValue = 1;
                                break;
                            }
                        }


                        heights[x, y] = ParametricSmoothStep(noiseValue, power);
                       //heights[x, y] = noiseValue;
                    }
                }
                else heights[x, y] = terrainData.GetHeight(x, y);
            }
        }
        return heights;
    }

    public void RestoreTerrain()
    {
        Terrain terrain = GetComponent<Terrain>();
        
        if (terrain != null && terrain.terrainData != null)
            terrain.terrainData.SetHeights(0, 0, TerrainHeights);
    }

    public static float ParametricSmoothStep(float x, float p)
    {
        x = Math.Clamp(x, 0f, 1f); // Ensure x is in [0,1]
        float t = MathF.Pow(x, p); // Apply exponent to bias towards small values
        return t * (3f - 2f * x);
    }
}
