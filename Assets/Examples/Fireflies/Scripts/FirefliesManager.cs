using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class FirefliesManager : MonoBehaviour
{
    public const float RADIUS = 0.8f;
    public const float ACCELERATION = 0.5f;
    public const float ENERGY_INTERACTION = 0.01f;
    public const float COLOR_DECRESE_OVER_TIME = 0.05f;
    public const float COLOR_INCRESE_OVER_TIME = 0.01f;
    
    [Header("Fireflies")]
    [SerializeField] private GameObject _prefabFirefly;
    [SerializeField] private int _nbFirefly;
    [SerializeField] private Vector2 _spawnAreaSize;
    [SerializeField] private int _maxNeighbors;
    
    [Header("Space partitioning")]
    [SerializeField] private bool _useSpacePartitioning;
    [SerializeField] private float _gridSize = 2;
    
    private int[] _grid;
    private int[] _gridFireflyCount;
    private int[] _potentialsNeighbors;
    private int[] _fireflyToGridCoord;
    private int _potentialsNeighborCount;
    private int _gridCellCountX;
    private int _gridCellCountY;
    
    private List<Transform> _instantiatedFireflies = new();
    private List<SpriteRenderer> _sprites = new();

    private Vector3[] _positions;
    private Vector2[] _velocities;
    private float[] _energies;
    private float[] _colorLerpValues;
    private bool[] _isEmittingLights;
    
    private int[] _neighborsIndex;
    private int[] _neighborsCount;

    private Color _noLightColor;
    private Color _lightColor;
    
    private void Start()
    {
        _instantiatedFireflies = new List<Transform>();
        
        _neighborsCount = new int[_nbFirefly];
        _neighborsIndex = new int[_nbFirefly * _maxNeighbors];
        _isEmittingLights = new bool[_nbFirefly];
        _colorLerpValues = new float[_nbFirefly];
        _energies = new float[_nbFirefly];
        _velocities = new Vector2[_nbFirefly];
        _positions = new Vector3[_nbFirefly];
        _potentialsNeighbors = new int[_nbFirefly];
        _fireflyToGridCoord = new int[_nbFirefly];
        
        _gridCellCountX = GridCellCountX();
        _gridCellCountY = GridCellCountY();
        
        _grid = new int[_nbFirefly * TotalCellCount()];
        _gridFireflyCount = new int[TotalCellCount()];

        _noLightColor = Color.gray;
        _lightColor = Color.yellow;
        
        for (int i = 0; i < _nbFirefly; i++)
        {
            GameObject instance = Instantiate(
                _prefabFirefly, 
                new Vector3(Random.Range(-_spawnAreaSize.x * 0.5f, _spawnAreaSize.x * 0.5f), Random.Range(-_spawnAreaSize.y * 0.5f, _spawnAreaSize.y * 0.5f), 0), 
                Quaternion.identity);
            
            _instantiatedFireflies.Add(instance.transform);
            _neighborsCount[i] = 0;
            _positions[i] = instance.transform.position;
            _velocities[i] = Vector2.zero;
            _energies[i] = Random.Range(0.0f, 1.0f);
            _isEmittingLights[i] = false;
            _colorLerpValues[i] = 0;
            _sprites.Add(instance.GetComponentInChildren<SpriteRenderer>());
            _sprites[^1].color = Color.Lerp(_noLightColor, _lightColor, 0);
            _fireflyToGridCoord[i] = 0;
        }
    }

    private void Update()
    {
        if (_nbFirefly == 0)
        {
            return;
        }

        for (int i = 0; i < TotalCellCount(); i++)
        {
            _gridFireflyCount[i] = 0;
        }
        
        // Position
        float dt = Time.deltaTime;
        const float friction = 0.999f;
        for (int i = 0; i < _nbFirefly; i++)
        {
            _neighborsCount[i] = 0;
            
            Vector2 vel = _velocities[i];
            Vector3 pos = _positions[i];
            Vector2 acc = new Vector2(Random.Range(-ACCELERATION, ACCELERATION), Random.Range(-ACCELERATION, ACCELERATION));
            vel = friction * vel + acc * dt;
            pos += (Vector3)vel * dt;

            if (pos.x < -_spawnAreaSize.x * 0.5f)
            {
                vel.x *= -0.7f;
                pos = new Vector3(-_spawnAreaSize.x * 0.5f, pos.y);
            }
            else if (pos.x > _spawnAreaSize.x * 0.5f)
            {
                vel.x *= -0.7f;
                pos = new Vector3(_spawnAreaSize.x * 0.5f, pos.y);
            }

            if (pos.y < -_spawnAreaSize.y * 0.5f)
            {
                vel.y *= -0.7f;
                pos = new Vector3(pos.x, -_spawnAreaSize.y * 0.5f);
            }
            else if (pos.y > _spawnAreaSize.y * 0.5f)
            {
                vel.y *= -0.7f;
                pos = new Vector3(pos.x, _spawnAreaSize.y * 0.5f);
            }

            _instantiatedFireflies[i].position = pos;
            _positions[i] = pos;
            _velocities[i] = vel;
            
            // Grid

            int gridIndex = GetGridIndexFromPosition(pos);
            _grid[gridIndex * _nbFirefly + _gridFireflyCount[gridIndex]++] = i;
            _fireflyToGridCoord[i] = gridIndex;
        }
        
        // Contact 
        for (int i = 0; i < _nbFirefly; i++)
        {
            Vector3 pos = _positions[i];

            GetPotentialNeighbors(_fireflyToGridCoord[i]);

            for(int j = 0; j < _potentialsNeighborCount; j++) 
            {
                int neighborIndex = _potentialsNeighbors[j];
                if (neighborIndex == i) continue; 
                
                if (Vector3.Distance(pos, _positions[neighborIndex]) <= RADIUS)
                {
                    _neighborsIndex[i * _maxNeighbors + _neighborsCount[i]++] = neighborIndex;
                }
            }
        }
        
        // Energy
        for (int i = 0; i < _nbFirefly; i++)
        {
            if (_isEmittingLights[i]) continue;
            
            int sum = 0;
            int neighborCount = _neighborsCount[i];
            for (int j = 0; j < neighborCount; j++)
            {
                int neighborIndex = _neighborsIndex[i * _maxNeighbors + j];
                if (Math.Abs(_colorLerpValues[neighborIndex] - 1) < Mathf.Epsilon) 
                { 
                    sum++;
                }
            }

            _energies[i] += sum * ENERGY_INTERACTION;
        }
        
        // Lights
        for (int i = 0; i < _nbFirefly; i++)
        {
            if (_isEmittingLights[i])
            {
                _sprites[i].color = Color.Lerp(_noLightColor, _lightColor, _colorLerpValues[i]);
                _colorLerpValues[i] -= COLOR_DECRESE_OVER_TIME;

                if (_colorLerpValues[i] <= 0)
                {
                    _isEmittingLights[i] = false;
                }
            }
            else
            {
                _energies[i] += COLOR_INCRESE_OVER_TIME;

                if (_energies[i] > 1)
                {
                    _energies[i] = 0;
                    _isEmittingLights[i] = true;
                    _colorLerpValues[i] = 1;
                }
            }
        }
    }
    
    private int GetGridIndexFromPosition(Vector3 position)
    {
        float offsetX = position.x + (_spawnAreaSize.x / 2f);
        float offsetY = position.y + (_spawnAreaSize.y / 2f); 
        
        int nx = _gridCellCountX;
        int ny = _gridCellCountY;
        
        int x = Mathf.FloorToInt(offsetX / _gridSize);
        int y = Mathf.FloorToInt(offsetY / _gridSize);
        
        x = Mathf.Clamp(x, 0, nx - 1);
        y = Mathf.Clamp(y, 0, ny - 1);

        return x + y * _gridCellCountX;
    }

    private Vector2Int GetGridCoordFromIndex(int gridIndex)
    {
        return new Vector2Int(gridIndex % _gridCellCountX, gridIndex / _gridCellCountX);
    }
    
    private void GetPotentialNeighbors(int gridIndex)
    {
        Vector2Int centerCoord = GetGridCoordFromIndex(gridIndex);
        int centerCoordX = centerCoord.x;
        int centerCoordY = centerCoord.y;
        _potentialsNeighborCount = 0;
        
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            { 
                int coordX = centerCoordX + i;
                int coordY = centerCoordY + j;
                int index =  coordX + coordY * _gridCellCountX;

                if (coordX >= 0 && coordX < _gridCellCountX && coordY >= 0 && coordY < _gridCellCountY)
                {
                    for (int k = 0; k < _gridFireflyCount[index]; k++)
                    {
                        _potentialsNeighbors[_potentialsNeighborCount++] = _grid[_nbFirefly * index + k];
                    }
                }
            }
        }
    }
    
    private int GridCellCountX()
    {
        float areaX = _spawnAreaSize.x;
        return Mathf.CeilToInt(areaX / _gridSize);
    }
    
    private int GridCellCountY()
    {
        float areaY = _spawnAreaSize.y; 
        return Mathf.CeilToInt(areaY / _gridSize);
    }

    private int TotalCellCount()
    {
        return _gridCellCountX * _gridCellCountY;
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.DrawWireCube(Vector3.zero, _spawnAreaSize);

        for (var i = 0; i < _nbFirefly; i++)
        {
            var firefly = _instantiatedFireflies[i];
            if (Selection.activeGameObject == firefly.gameObject)
            {
                for (int j = 0; j < _neighborsCount[i]; j++)
                {
                    var otherIndex = _neighborsIndex[i * _maxNeighbors + j];
                    Gizmos.DrawLine(_positions[i], _positions[otherIndex]);
                }
            }
        }

        var gridIndex = GetGridIndexFromPosition(GetMouseWorldPosition());

        for (int i = 0; i < _gridFireflyCount[gridIndex]; i++)
        {
            Gizmos.DrawWireCube(_positions[_grid[gridIndex * _nbFirefly + i]], Vector3.one * 0.2f);
        }
    }
    
    public Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            return hit.point;
        }
        
        float distance;
        if (new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
}
