using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices; 

public class BoidComputeManager : MonoBehaviour
{
    struct BoidData
    {
        public Vector2 position; // Not use but to respect data size
        public Vector2 velocity;
        public Vector2 direction; // Not use but to respect data size
    }

    [Header("Compute Shader")]
    public ComputeShader boidComputeShader;

    [Header("Rendering")]
    public Material boidMaterial;

    [Header("Settings")]
    public int boidCount = 10000;
    public float speed = 5f;
    public float neighborRadius = 5f;
    public float avoidRadius = 2f;
    public Vector2 simulationSize = new Vector2(100, 100);

    [Header("Weights")]
    public float cohesionWeight = 1;
    public float alignmentWeight = 1;
    public float separationWeight = 1;
    public float fovAngle = 270f;
    
    [Header("Rendering Colors")]
    public Color slowColor = Color.blue;
    public Color fastColor = Color.red;

    // Compute Buffers
    private ComputeBuffer _boidBuffer;
    private ComputeBuffer _gridHeadsBuffer;
    private ComputeBuffer _gridNextBuffer;
    
    // Commmand Buffer
    private GraphicsBuffer _commandBuffer;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;

    private BoidData[] _boidDataArray;

    private int _gridCols, _gridRows;
    private float _gridCellSize;
    private Vector2 _gridBottomLeft;
    private float _fovThreshold;

    private int _csMainKernelID;
    private int _csClearKernelID;
    private int _csUpdateKernelID;

    private MaterialPropertyBlock _props;
    
    private Mesh _boidMesh;
    
    private void Start()
    {
        InitializeSimulation();
        InitializeRendering();
    }

    private void InitializeSimulation()
    {
        _gridCellSize = neighborRadius;
        _gridCols = Mathf.CeilToInt(simulationSize.x / _gridCellSize);
        _gridRows = Mathf.CeilToInt(simulationSize.y / _gridCellSize);
        _gridBottomLeft = -simulationSize / 2f;
        _fovThreshold = Mathf.Cos(fovAngle * 0.5f * Mathf.Deg2Rad);

        // CPU Initialization
        _boidDataArray = new BoidData[boidCount];
        for (int i = 0; i < boidCount; i++)
        {
            _boidDataArray[i].position = new Vector2(
                Random.Range(-simulationSize.x / 2f, simulationSize.x / 2f),
                Random.Range(-simulationSize.y / 2f, simulationSize.y / 2f)
            );
            _boidDataArray[i].velocity = Random.insideUnitCircle.normalized * speed;
            _boidDataArray[i].direction = _boidDataArray[i].velocity.normalized;
        }

        // ComputeBuffers
        _boidBuffer = new ComputeBuffer(boidCount, Marshal.SizeOf(typeof(BoidData)));
        _boidBuffer.SetData(_boidDataArray);

        _gridHeadsBuffer = new ComputeBuffer(_gridCols * _gridRows, sizeof(int));
        _gridNextBuffer = new ComputeBuffer(boidCount, sizeof(int));

        // Kernels
        _csClearKernelID = boidComputeShader.FindKernel("CSClearGrid");
        _csUpdateKernelID = boidComputeShader.FindKernel("CSUpdateGrid");
        _csMainKernelID = boidComputeShader.FindKernel("CSMain");

        // Uniforms
        boidComputeShader.SetInt("boidCount", boidCount);
        boidComputeShader.SetFloat("speed", speed);
        boidComputeShader.SetFloat("neighborRadius", neighborRadius);
        boidComputeShader.SetFloat("avoidRadius", avoidRadius);
        boidComputeShader.SetVector("simSize", simulationSize);
        boidComputeShader.SetFloat("fovThreshold", _fovThreshold);
        boidComputeShader.SetFloat("wCoh", cohesionWeight);
        boidComputeShader.SetFloat("wAli", alignmentWeight);
        boidComputeShader.SetFloat("wSep", separationWeight);
        boidComputeShader.SetInt("gridCols", _gridCols);
        boidComputeShader.SetInt("gridRows", _gridRows);
        boidComputeShader.SetVector("bottomLeft", _gridBottomLeft);

        // Grid Clear
        boidComputeShader.SetBuffer(_csClearKernelID, "gridHeads", _gridHeadsBuffer);

        // Grid Update
        boidComputeShader.SetBuffer(_csUpdateKernelID, "boids", _boidBuffer);
        boidComputeShader.SetBuffer(_csUpdateKernelID, "gridHeads", _gridHeadsBuffer);
        boidComputeShader.SetBuffer(_csUpdateKernelID, "gridNext", _gridNextBuffer);

        // Main Sim
        boidComputeShader.SetBuffer(_csMainKernelID, "boids", _boidBuffer);
        boidComputeShader.SetBuffer(_csMainKernelID, "gridHeads", _gridHeadsBuffer);
        boidComputeShader.SetBuffer(_csMainKernelID, "gridNext", _gridNextBuffer);
    }

    private void InitializeRendering()
    {
        if (_boidMesh == null) _boidMesh = CreateBoidMesh();

        boidMaterial.enableInstancing = true; 
        _props = new MaterialPropertyBlock();

        _commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
    
        _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        _commandData[0].indexCountPerInstance = (uint)_boidMesh.GetIndexCount(0);
        _commandData[0].instanceCount = (uint)boidCount;
        _commandData[0].startIndex = (uint)_boidMesh.GetIndexStart(0);
        _commandData[0].baseVertexIndex = (uint)_boidMesh.GetBaseVertex(0);
        _commandData[0].startInstance = 0;

        _commandBuffer.SetData(_commandData);
    }

    private void Update()
    {
        RunSimulation();
        RenderBoids();
    }

    private void RunSimulation()
    {
        // Reset Grid
        int gridGroups = Mathf.CeilToInt((_gridCols * _gridRows) / 64f);
        boidComputeShader.Dispatch(_csClearKernelID, gridGroups, 1, 1);

        // Update Grid
        int boidGroups = Mathf.CeilToInt(boidCount / 64f);
        boidComputeShader.Dispatch(_csUpdateKernelID, boidGroups, 1, 1);

        // Simulation Boids
        boidComputeShader.SetFloat("deltaTime", Time.deltaTime);
        int groups = Mathf.CeilToInt(boidCount / 64f);
        boidComputeShader.Dispatch(_csMainKernelID, groups, 1, 1);
    }

    private void RenderBoids()
    {
        RenderParams rp = new RenderParams(boidMaterial);
    
        rp.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
    
        rp.shadowCastingMode = ShadowCastingMode.Off;
        rp.receiveShadows = false;

        _props.SetColor("_SlowColor", slowColor);
        _props.SetColor("_FastColor", fastColor);
        _props.SetBuffer("_BoidBuffer", _boidBuffer);
        _props.SetFloat("_MaxSpeedRef", Mathf.Max(speed, 0.001f));
        rp.matProps = _props;

        Graphics.RenderMeshIndirect(rp, _boidMesh, _commandBuffer, 1);
    }

    private void OnDestroy()
    {
        _boidBuffer?.Release();
        _gridHeadsBuffer?.Release();
        _gridNextBuffer?.Release();
        _commandBuffer?.Release(); 
    }

    private Mesh CreateBoidMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = {
            new(0, 0.15f, 0),
            new(0.15f, -0.15f, 0),
            new(-0.15f, -0.15f, 0)
        };
        int[] triangles = { 0, 1, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }
}