using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private GameObject _boidPrefab;
    [SerializeField] private int _boidCount;
    [SerializeField] private float _spawnRadius;
    
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _neighborRadius = 5f; 
    [SerializeField] private float _avoidRadius = 2f; 
    [SerializeField] private float _movementRadius = 100f; 
    [SerializeField] private int _maxNeighborCount = 100;
    
    [SerializeField] private float _cohesionWeight = 1; 
    [SerializeField] private float _alligmentWeight = 1; 
    [SerializeField] private float _separationWeight = 1; 
    
    [SerializeField] private float _fovAngle = 270f;
    private float _fovThreshold;
    
    private Transform[] _boids;
    private Vector2[] _velocities;
    private Vector2[] _positions;
    private Vector2[] _directions;
    private Vector2[] _influences;
    private int[] _neighborsCount;
    private int[] _neighborsIndex;
    private float[] _neighborsDistance;
    
    private int[] _gridHeads;   
    private int[] _gridNext;      
    private int _gridCols, _gridRows;
    private float _gridCellSize;
    private Vector2 _gridBottomLeft;

    [SerializeField] private Vector2 _simulationSize = new Vector2(100, 100);

    private void Start()
    {
        _boids =  new Transform[_boidCount];
        _velocities = new Vector2[_boidCount];
        _positions = new Vector2[_boidCount];
        _directions = new Vector2[_boidCount];
        _influences = new Vector2[_boidCount];
        _neighborsCount =  new int[_boidCount];
        _neighborsIndex = new int[_boidCount * _maxNeighborCount];
        _neighborsDistance = new float[_boidCount * _maxNeighborCount];
        
        for (int i = 0; i < _boidCount; i++)
        {
            float rx = Random.Range(-_simulationSize.x / 2f, _simulationSize.x / 2f);
            float ry = Random.Range(-_simulationSize.y / 2f, _simulationSize.y / 2f);
            Vector2 randomPos = new Vector2(rx, ry);
            Quaternion randomRot = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            
            GameObject boid = Instantiate(_boidPrefab, randomPos, randomRot);
            boid.name = "Boid_" + i;
            boid.transform.SetParent(transform);
            _boids[i] = boid.transform;
            _positions[i] = randomPos;
            _velocities[i] = boid.transform.up * _speed;
            _directions[i] = boid.transform.up;
        }
        
        // Grid
        _gridCellSize = _neighborRadius;
        _gridCols = Mathf.CeilToInt(_simulationSize.x / _gridCellSize);
        _gridRows = Mathf.CeilToInt(_simulationSize.y / _gridCellSize);
        
        _gridHeads = new int[_gridCols * _gridRows];
        _gridNext = new int[_boidCount];
        _gridBottomLeft = -_simulationSize / 2f;
        
        // FOV
        _fovThreshold = Mathf.Cos(_fovAngle * 0.5f * Mathf.Deg2Rad);
    }

    private void FixedUpdate()
    {
        UpdateGrid();

        // Find Neighbors
        FindNeighbors();

        // Compute influence
        ComputeInfluence();
        
        // Apply influence
        ApplyInfluenceAndMovement();
    }

    private void FindNeighbors()
    {
        float neighborRadiusSq = _neighborRadius * _neighborRadius;
        float halfWidth = _simulationSize.x * 0.5f;
        float halfHeight = _simulationSize.y * 0.5f;

        for (int i = 0; i < _boidCount; i++)
        {
            int count = 0;
            Vector2 boidPos = _positions[i];
            Vector2 boidDir = _directions[i];

            int gridX = Mathf.FloorToInt((boidPos.x - _gridBottomLeft.x) / _gridCellSize);
            int gridY = Mathf.FloorToInt((boidPos.y - _gridBottomLeft.y) / _gridCellSize);

            for (int x = gridX - 1; x <= gridX + 1; x++)
            {
                if (x < 0 || x >= _gridCols) continue;
                for (int y = gridY - 1; y <= gridY + 1; y++)
                {
                    if (y < 0 || y >= _gridRows) continue;

                    int cellIndex = x + y * _gridCols;
                    int neighborIndex = _gridHeads[cellIndex];

                    while (neighborIndex != -1 && count < _maxNeighborCount)
                    {
                        if (neighborIndex != i)
                        {
                            // Inlining tor postion
                            float dx = _positions[neighborIndex].x - boidPos.x;
                            if (dx > halfWidth) dx -= _simulationSize.x;
                            else if (dx < -halfWidth) dx += _simulationSize.x;

                            float dy = _positions[neighborIndex].y - boidPos.y;
                            if (dy > halfHeight) dy -= _simulationSize.y;
                            else if (dy < -halfHeight) dy += _simulationSize.y;

                            float dSq = dx * dx + dy * dy;

                            if (dSq < neighborRadiusSq)
                            {
                                float d = Mathf.Sqrt(dSq);
                                float invD = 1.0f / (d + 0.0001f);
                                float dot = (boidDir.x * dx * invD) + (boidDir.y * dy * invD);

                                if (dot >= _fovThreshold)
                                {
                                    int flatIndex = i * _maxNeighborCount + count;
                                    _neighborsIndex[flatIndex] = neighborIndex;
                                    _neighborsDistance[flatIndex] = d;
                                    count++;
                                }
                            }
                        }
                        neighborIndex = _gridNext[neighborIndex];
                    }
                }
            }
            _neighborsCount[i] = count;
        }
    }

   private void ComputeInfluence()
    {
        for (int i = 0; i < _boidCount; i++)
        {
            Vector2 influence = Vector2.zero;
            int neighborCount = _neighborsCount[i];
            
            int boidOffset = i * _maxNeighborCount;

            // Alignment
            if (neighborCount > 0)
            {
                Vector2 alignmentMove = Vector2.zero;
                for (int j = 0; j < neighborCount; j++)
                {
                    int neighborIndex = _neighborsIndex[boidOffset + j];
                    alignmentMove += _directions[neighborIndex];
                }
                
                alignmentMove /= neighborCount;
                alignmentMove *= _alligmentWeight;
                
                if (alignmentMove.sqrMagnitude > _alligmentWeight * _alligmentWeight)
                {
                    alignmentMove.Normalize();
                    alignmentMove *= _alligmentWeight;
                }

                influence += alignmentMove;
            }
            else
            {
                influence += _directions[i];
            }
            
            // Separation 
            if (neighborCount > 0)
            {
                Vector2 separationMove = Vector2.zero;
                int avoidCount = 0;
                
                for (int j = 0; j < neighborCount; j++)
                {
                    int flatIndex = boidOffset + j;
                    float dist = _neighborsDistance[flatIndex];
                        
                    if (dist < _avoidRadius)
                    {
                        avoidCount++;
                        int neighborIndex = _neighborsIndex[flatIndex];
                            
                        Vector2 diff = -GetToroidalOffset(_positions[neighborIndex], _positions[i]);
                
                        float strength = 1f - (dist / _avoidRadius); 
                        separationMove += diff.normalized * (strength * strength); 
                    }
                }
                
                if (avoidCount > 0)
                {
                    influence += (separationMove / avoidCount) * _separationWeight;
                }
            }

            // Cohesion
            if (neighborCount > 0)
            {
                Vector2 cohesionMove = Vector2.zero;
                for (int j = 0; j < neighborCount; j++)
                {
                    int neighborIndex = _neighborsIndex[boidOffset + j];
                    cohesionMove += GetToroidalOffset(_positions[neighborIndex], _positions[i]);
                }
                influence += (cohesionMove / neighborCount) * _cohesionWeight;
            }

            _influences[i] = influence;
        }
    }

    private void ApplyInfluenceAndMovement()
    {
        for (int i = 0; i < _boidCount; i++)
        {
            _velocities[i] += _influences[i] * Time.deltaTime;

            if (_velocities[i].magnitude > _speed)
            {
                _velocities[i] = _velocities[i].normalized * _speed;
            }

            _positions[i] += _velocities[i] * Time.deltaTime;

            float halfWidth = _simulationSize.x / 2f;
            float halfHeight = _simulationSize.y / 2f;

            if (_positions[i].x > halfWidth) _positions[i].x = -halfWidth;
            else if (_positions[i].x < -halfWidth) _positions[i].x = halfWidth;

            if (_positions[i].y > halfHeight) _positions[i].y = -halfHeight;
            else if (_positions[i].y < -halfHeight) _positions[i].y = halfHeight;

            _boids[i].position = _positions[i];
    
            if (_velocities[i] != Vector2.zero)
            {
                _boids[i].up = Vector2.Lerp(_boids[i].up, _velocities[i].normalized, Time.deltaTime * 10f);
                _directions[i] = _boids[i].up;
            }
        }
    }

    private void UpdateGrid()
    {
        for (int i = 0; i < _gridHeads.Length; i++) _gridHeads[i] = -1;

        for (int i = 0; i < _boidCount; i++)
        {
            int cellIndex = GetCellIndex(_positions[i]);
            if (cellIndex >= 0 && cellIndex < _gridHeads.Length)
            {
                _gridNext[i] = _gridHeads[cellIndex];
                _gridHeads[cellIndex] = i;
            }
        }
    }

    private int GetCellIndex(Vector2 pos)
    {
        int x = Mathf.FloorToInt((pos.x - _gridBottomLeft.x) / _gridCellSize);
        int y = Mathf.FloorToInt((pos.y - _gridBottomLeft.y) / _gridCellSize);
        
        if (x < 0 || x >= _gridCols || y < 0 || y >= _gridRows) return -1;
        return x + y * _gridCols;
    }
    
    private Vector2 GetToroidalOffset(Vector2 target, Vector2 current)
    {
        Vector2 diff = target - current;

        if (Mathf.Abs(diff.x) > _simulationSize.x / 2f)
        {
            diff.x -= Mathf.Sign(diff.x) * _simulationSize.x;
        }

        if (Mathf.Abs(diff.y) > _simulationSize.y / 2f)
        {
            diff.y -= Mathf.Sign(diff.y) * _simulationSize.y;
        }

        return diff;
    }
}
