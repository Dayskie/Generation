using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public enum Drawmode { NoiseMap, ColourMap, Mesh};
    public Drawmode drawMode;

    const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarety;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrianTypes[] regions;

    public void GenerateMap() {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarety, offset);

        Color[] coloutMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].HeightValue) {
                        coloutMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == Drawmode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if (drawMode == Drawmode.ColourMap) {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(coloutMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == Drawmode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrianMesh (noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(coloutMap, mapChunkSize, mapChunkSize));
        }
    }

    void OnValidate() {
        if (lacunarety < 1) {
            lacunarety = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }
}

[System.Serializable]
public struct TerrianTypes {
    public string name;
    public float HeightValue;
    public Color colour;
}