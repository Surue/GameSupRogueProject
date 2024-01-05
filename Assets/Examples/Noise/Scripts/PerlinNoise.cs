using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    [SerializeField] private int _mapSize = 100;
    [SerializeField] private int _seed = 0;
    [SerializeField] private float _scale = 1000;
    [SerializeField] private int _octave = 5;
    [SerializeField] private float _persistance = 2.3f;
    [SerializeField] private float _lacunarity = 3.07f;
    [SerializeField] private float _height = 4;
    [SerializeField] private bool _update;

    private float[,] _map;

    private bool _isRunning = false;

    private Vector2 _offset = Vector2.zero;
    
    private void Start()
    {
        _isRunning = true;

        _map = new float[_mapSize, _mapSize];
        Generate();
    }

    private void Generate()
    {
        Noise.GenerateNoiseMap(_map, _mapSize, _mapSize, _seed, _scale, _octave, _persistance, _lacunarity, _offset);
    }

    private void Update()
    {
        if (!_update)
        {
            return;
        }
        _offset += Vector2.one * Time.deltaTime;
        Generate();
    }

    private void OnDrawGizmos()
    {
        if (!_isRunning) return;
        
        for (int x = 0; x < _mapSize; x++) {
            for (int y = 0; y < _mapSize; y++) {
                Gizmos.color = new Color(_map[x, y], _map[x, y], _map[x, y]);
                Gizmos.DrawCube(new Vector3(x, _map[x, y] * _height, y), Vector3.one);
            }
        }
    }
}

public static class Noise {

    public static void GenerateNoiseMap(float[,] noiseMap, int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        System.Random prng = new System.Random(seed);
        Vector2[] octavesOffset = new Vector2[octaves];
        for(int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-10000, 10000) + offset.x;
            float offsetY = prng.Next(-10000, 10000) + offset.y;
            octavesOffset[i] = new Vector2(offsetX, offsetY);
        }

        if(scale <= 0) {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth * 0.5f;
        float halfHeight = mapHeight * 0.5f;

        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;

                float noiseHeight = 0;

                float tmpSampleX = (x - halfWidth);
                float tmpSampleY = (y - halfHeight);
                for(int i = 0; i < octaves; i++) {
                    float sampleX = tmpSampleX / scale * frequency + octavesOffset[i].x;
                    float sampleY = tmpSampleY / scale * frequency + octavesOffset[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight) {
                    maxNoiseHeight = noiseHeight;
                } else if(noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        //Normalize map
        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapWidth; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
    }
}

