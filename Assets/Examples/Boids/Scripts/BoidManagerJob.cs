using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class BoidManagerJob : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject _boidPrefab;
    [SerializeField] private int _boidCount = 500;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _neighborRadius = 5f;
    [SerializeField] private float _avoidRadius = 2f;
    [SerializeField] private int _maxNeighborCount = 50;
    [SerializeField] private Vector2 _simulationSize = new Vector2(100, 100);

    [Header("Weights")]
    [SerializeField] private float _cohesionWeight = 1;
    [SerializeField] private float _alligmentWeight = 1;
    [SerializeField] private float _separationWeight = 1;
    [SerializeField] private float _fovAngle = 270f;

    private Transform[] _boids;
    private float _fovThreshold;

    // Raw data
    private NativeArray<Vector2> _positions;
    private NativeArray<Vector2> _velocities;
    private NativeArray<Vector2> _directions;
    private NativeArray<Vector2> _influences;
    
    // Grid Data
    private NativeArray<int> _gridHeads;
    private NativeArray<int> _gridNext;

    private void Start()
    {
        _boids = new Transform[_boidCount];
        _positions = new NativeArray<Vector2>(_boidCount, Allocator.Persistent);
        _velocities = new NativeArray<Vector2>(_boidCount, Allocator.Persistent);
        _directions = new NativeArray<Vector2>(_boidCount, Allocator.Persistent);
        _influences = new NativeArray<Vector2>(_boidCount, Allocator.Persistent);

        _gridNext = new NativeArray<int>(_boidCount, Allocator.Persistent);
        float cellSize = _neighborRadius;
        int cols = Mathf.CeilToInt(_simulationSize.x / cellSize);
        int rows = Mathf.CeilToInt(_simulationSize.y / cellSize);
        _gridHeads = new NativeArray<int>(cols * rows, Allocator.Persistent);

        _fovThreshold = Mathf.Cos(_fovAngle * 0.5f * Mathf.Deg2Rad);

        for (int i = 0; i < _boidCount; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-_simulationSize.x/2, _simulationSize.x/2), Random.Range(-_simulationSize.y/2, _simulationSize.y/2));
            _boids[i] = Instantiate(_boidPrefab, pos, Quaternion.identity).transform;
            _boids[i].transform.parent = transform;
            _boids[i].name = "Boid_" + i;
            _positions[i] = pos;
            _velocities[i] = Random.insideUnitCircle.normalized * _speed;
            _directions[i] = _velocities[i].normalized;
        }
    }

    private void OnDestroy()
    {
        // Free memory
        if (_positions.IsCreated) _positions.Dispose();
        if (_velocities.IsCreated) _velocities.Dispose();
        if (_directions.IsCreated) _directions.Dispose();
        if (_influences.IsCreated) _influences.Dispose();
        if (_gridHeads.IsCreated) _gridHeads.Dispose();
        if (_gridNext.IsCreated) _gridNext.Dispose();
    }

    private void FixedUpdate()
    {
        // pdate Grid
        for (int i = 0; i < _gridHeads.Length; i++) _gridHeads[i] = -1;
        float cellSize = _neighborRadius;
        Vector2 bottomLeft = -_simulationSize / 2f;
        int cols = Mathf.CeilToInt(_simulationSize.x / cellSize);
        int rows = Mathf.CeilToInt(_simulationSize.y / cellSize);

        for (int i = 0; i < _boidCount; i++)
        {
            int gx = Mathf.FloorToInt((_positions[i].x - bottomLeft.x) / cellSize);
            int gy = Mathf.FloorToInt((_positions[i].y - bottomLeft.y) / cellSize);
            if (gx >= 0 && gx < cols && gy >= 0 && gy < rows) {
                int idx = gx + gy * cols;
                _gridNext[i] = _gridHeads[idx];
                _gridHeads[idx] = i;
            }
        }

        // Influence Job
        var computeJob = new BoidComputeJob
        {
            Positions = _positions,
            Directions = _directions,
            GridHeads = _gridHeads,
            GridNext = _gridNext,
            Influences = _influences,
            SimSize = _simulationSize,
            CellSize = cellSize,
            GridCols = cols, GridRows = rows,
            BottomLeft = bottomLeft,
            NeighborRadius = _neighborRadius,
            AvoidRadius = _avoidRadius,
            FovThreshold = _fovThreshold,
            MaxNeighbors = _maxNeighborCount,
            W_Coh = _cohesionWeight, W_Ali = _alligmentWeight, W_Sep = _separationWeight
        };

        JobHandle handle = computeJob.Schedule(_boidCount, 64);
        handle.Complete(); 
        
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        float dt = Time.deltaTime;
        float halfW = _simulationSize.x / 2;
        float halfH = _simulationSize.y / 2;

        for (int i = 0; i < _boidCount; i++)
        {
            Vector2 vel = _velocities[i] + _influences[i] * dt;
            if (vel.magnitude > _speed) vel = vel.normalized * _speed;

            Vector2 pos = _positions[i] + vel * dt;

            // Wrapping
            if (pos.x > halfW) pos.x = -halfW; else if (pos.x < -halfW) pos.x = halfW;
            if (pos.y > halfH) pos.y = -halfH; else if (pos.y < -halfH) pos.y = halfH;

            _positions[i] = pos;
            _velocities[i] = vel;
            _boids[i].position = pos;
            
            if (vel != Vector2.zero) {
                _boids[i].up = Vector2.Lerp(_boids[i].up, vel.normalized, dt * 10f);
                _directions[i] = _boids[i].up;
            }
        }
    }

    [BurstCompile]
    struct BoidComputeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector2> Positions;
        [ReadOnly] public NativeArray<Vector2> Directions;
        [ReadOnly] public NativeArray<int> GridHeads;
        [ReadOnly] public NativeArray<int> GridNext;
        
        public NativeArray<Vector2> Influences;

        public Vector2 SimSize;
        public float CellSize, NeighborRadius, AvoidRadius, FovThreshold;
        public int GridCols, GridRows, MaxNeighbors;
        public Vector2 BottomLeft;
        public float W_Coh, W_Ali, W_Sep;

        public void Execute(int i)
        {
            Vector2 position = Positions[i];
            Vector2 direction = Directions[i];
            Vector2 cohesionMove = Vector2.zero;
            Vector2 alignmentMove = Vector2.zero;
            Vector2 separationMove = Vector2.zero;
            int neighborCount = 0;
            int avoidCount = 0;

            int gx = (int)((position.x - BottomLeft.x) / CellSize);
            int gy = (int)((position.y - BottomLeft.y) / CellSize);

            for (int x = gx - 1; x <= gx + 1; x++) {
                if (x < 0 || x >= GridCols) continue;
                for (int y = gy - 1; y <= gy + 1; y++) {
                    if (y < 0 || y >= GridRows) continue;
                    
                    int neighborIdx = GridHeads[x + y * GridCols];
                    while (neighborIdx != -1) {
                        if (neighborIdx != i) {
                            Vector2 offset = GetToroidalOffset(Positions[neighborIdx], position);
                            float dSq = offset.x * offset.x + offset.y * offset.y;

                            if (dSq < NeighborRadius * NeighborRadius) {
                                float dist = Mathf.Sqrt(dSq);
                                if (Vector2.Dot(direction, offset / (dist + 0.001f)) >= FovThreshold) {
                                    neighborCount++;
                                    alignmentMove += Directions[neighborIdx];
                                    cohesionMove += offset; 

                                    if (dist < AvoidRadius) {
                                        avoidCount++;
                                        float s = 1f - (dist / AvoidRadius);
                                        separationMove -= offset.normalized * (s * s);
                                    }
                                }
                            }
                        }
                        neighborIdx = GridNext[neighborIdx];
                        if (neighborCount >= MaxNeighbors) break;
                    }
                }
            }

            Vector2 influence = Vector2.zero;
            if (neighborCount > 0) {
                influence += (cohesionMove / neighborCount) * W_Coh;
                influence += (alignmentMove / neighborCount) * W_Ali;
            } else {
                influence += direction;
            }
            if (avoidCount > 0) influence += (separationMove / avoidCount) * W_Sep;

            Influences[i] = influence;
        }

        private Vector2 GetToroidalOffset(Vector2 target, Vector2 current) {
            Vector2 diff = target - current;
            if (diff.x > SimSize.x * 0.5f) diff.x -= SimSize.x;
            else if (diff.x < -SimSize.x * 0.5f) diff.x += SimSize.x;
            if (diff.y > SimSize.y * 0.5f) diff.y -= SimSize.y;
            else if (diff.y < -SimSize.y * 0.5f) diff.y += SimSize.y;
            return diff;
        }
    }
}