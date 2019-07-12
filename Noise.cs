using System.Collections;
using UnityEngine;

public static class Noise {

    public static float[,] GenerateNoiseMap(int MapWidth, int MapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[MapWidth, MapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offSetX = prng.Next(-100000, 100000) + offset.x;
            float offSetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offSetX, offSetY);
        }

        if (scale <= 0 ) {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = MapWidth / 2f;
        float halfHeight = MapHeight / 2f;

        for (int y = 0; y < MapHeight; y++) {
            for (int x = 0; x < MapWidth; x++) {

                float amplitude = 1;
                float frequancy = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequancy + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequancy + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequancy *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            {
                
            }
        }

        return noiseMap;
    }

}
