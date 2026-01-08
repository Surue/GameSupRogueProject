using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    #region Fields
    [SerializeField] private int _mapSize = 100;
    [SerializeField] private int _seed = 0;
    [SerializeField] private float _scale = 1000;
    [SerializeField] private int _octave = 5;
    [SerializeField] private float _persistance = 2.3f;
    [SerializeField] private float _lacunarity = 3.07f;
    [SerializeField] private float _height = 4;
    [SerializeField] private bool _update;
    
    [Header("Rolloff/Masking")]
    [SerializeField] private bool _useRolloff = true;
    [SerializeField] public AnimationCurve _heightRolloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Height Remapping")]
    [SerializeField] private bool _useHeightRemapping = true;
    [SerializeField] public AnimationCurve _heightCurve = AnimationCurve.Linear(0, 0, 1, 1); 

    private float[,] _map;

    private bool _isRunning = false;

    private Vector2 _offset = Vector2.zero;
    #endregion
    
    private void Start()
    {
        _isRunning = true;

        int size = Mathf.Max(1, _mapSize);
        _map = new float[size, size];
        Generate();
    }

    private void Generate()
    {
        int size = Mathf.Max(1, _mapSize);
        _map = new float[size, size];
        
        Noise.GenerateNoiseMap(
            _map, size, size, _seed, _scale, _octave, 
            _persistance, _lacunarity, _offset,
            _useRolloff, _heightRolloff,
            _useHeightRemapping, _heightCurve
        );
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

    private void OnValidate()
    {
        if (!Application.isPlaying) return; 
        
        Generate();
    }

    private void OnDrawGizmos()
    {
        if (!_isRunning || _map == null) return;
        
        Mesh mesh = new Mesh();
        
        int size = _mapSize;
        
        Vector3[] vertices = new Vector3[size * size];
        int[] triangles = new int[(size - 1) * (size - 1) * 6]; 
        Color[] colors = new Color[size * size];

        int vertIndex = 0;
        int triIndex = 0;

        for (int z = 0; z < size; z++) 
        {
            for (int x = 0; x < size; x++) 
            {
                float heightValue = _map[x, z] * _height;
                vertices[vertIndex] = new Vector3(x, heightValue, z);

                if (x < size - 1 && z < size - 1) 
                {
                    int bottomLeft = vertIndex;
                    int bottomRight = vertIndex + 1;
                    int topLeft = vertIndex + size;
                    int topRight = vertIndex + size + 1;

                    triangles[triIndex] = bottomLeft;
                    triangles[triIndex + 1] = topLeft;
                    triangles[triIndex + 2] = bottomRight;

                    triangles[triIndex + 3] = bottomRight;
                    triangles[triIndex + 4] = topLeft;
                    triangles[triIndex + 5] = topRight;

                    triIndex += 6;
                }

                vertIndex++;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        
        mesh.RecalculateNormals();

        Gizmos.color = Color.white; 

        Gizmos.matrix = transform.localToWorldMatrix;
        
        Gizmos.DrawMesh(mesh);
    }
}

public static class Noise {

    public static void GenerateNoiseMap(float[,] noiseMap, int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, bool useRolloff, AnimationCurve heightRolloff, bool useRemapping, AnimationCurve heightCurve) {
        System.Random prng = new System.Random(seed);
        Vector2[] octavesOffset = new Vector2[octaves];
        
        for(int i = 0; i < octaves; i++) 
        {
            float offsetX = prng.Next(-10000, 10000) + offset.x;
            float offsetY = prng.Next(-10000, 10000) + offset.y;
            octavesOffset[i] = new Vector2(offsetX, offsetY);
        }

        if(scale <= 0) 
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth * 0.5f;
        float halfHeight = mapHeight * 0.5f;

        for(int y = 0; y < mapHeight; y++) 
        {
            for(int x = 0; x < mapWidth; x++) 
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                float tmpSampleX = (x - halfWidth);
                float tmpSampleY = (y - halfHeight);
                for(int i = 0; i < octaves; i++) 
                {
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
        
        // Rolloff
        if (useRolloff)
        {
            float maxCornerDist = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);

            for(int y = 0; y < mapHeight; y++) 
            {
                for(int x = 0; x < mapWidth; x++) 
                {
                    
                    float tmpSampleX = (x - halfWidth); 
                    float tmpSampleY = (y - halfHeight);
                    
                    // Circular multiplier
                    float currentDistRadial = Mathf.Sqrt(tmpSampleX * tmpSampleX + tmpSampleY * tmpSampleY);
                    float normalizedDistRadial = currentDistRadial / maxCornerDist;
                    
                    float radialMultiplier = heightRolloff.Evaluate(normalizedDistRadial);

                    // Rectangular multiplier
                    float normalizedDistX = Mathf.Abs(tmpSampleX) / halfWidth; 
                    float normalizedDistY = Mathf.Abs(tmpSampleY) / halfHeight;
                    
                    float normalizedDistRect = Mathf.Max(normalizedDistX, normalizedDistY); 
                    
                    float edgeMultiplier = 1f - normalizedDistRect;

                    // Final multiplier
                    float combinedMultiplier = Mathf.Min(radialMultiplier, edgeMultiplier);

                    // Apply rolloff
                    noiseMap[x, y] *= combinedMultiplier;
                }
            }
        }
        
        // Height remapping 
        if (useRemapping)
        {
            for(int y = 0; y < mapHeight; y++) 
            {
                for(int x = 0; x < mapWidth; x++) 
                {
                    
                    float currentHeight = noiseMap[x, y];
                    
                    float remappedHeight = heightCurve.Evaluate(currentHeight);

                    noiseMap[x, y] = remappedHeight;
                }
            }
        }
    }
}

