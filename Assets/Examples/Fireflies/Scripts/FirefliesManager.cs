using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class FirefliesManager : MonoBehaviour
{
    public const float RADIUS = 1.5f;
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
    
    private List<Transform> _instantiatedFireflies = new();
    private List<SpriteRenderer> _sprites = new();

    private Vector3[] _positions;
    private Vector2[] _velocities;
    private float[] _energies;
    private float[] _colorLerpValues;
    private bool[] _isEmittingLights ;
    
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
        }
    }

    private void Update()
    {
        if (_nbFirefly == 0)
        {
            return;
        }
        
        // Position
        float dt = Time.deltaTime;
        const float friction = 0.999f;
        for (int i = 0; i < _nbFirefly; i++)
        {
            Vector2 vel = _velocities[i];
            Vector3 pos = _positions[i];
            Vector2 acc = new Vector2(Random.Range(-ACCELERATION, ACCELERATION), Random.Range(-ACCELERATION, ACCELERATION));
            vel = friction * vel + acc * dt;
            pos += (Vector3)vel * dt;

            if (pos.x < -9)
            {
                vel.x *= -0.7f;
                pos = new Vector3(-9, pos.y);
            }
            else if (pos.x > 9)
            {
                vel.x *= -0.7f;
                pos = new Vector3(9, pos.y);
            }

            if (pos.y < -5)
            {
                vel.y *= -0.7f;
                pos = new Vector3(pos.x, -5);
            }
            else if (pos.y > 5)
            {
                vel.y *= -0.7f;
                pos = new Vector3(pos.x, 5);
            }

            _instantiatedFireflies[i].position = pos;
            _positions[i] = pos;
            _velocities[i] = vel;
        }

        for (int i = 0; i < _nbFirefly; i++)
        {
            _neighborsCount[i] = 0;
        }
        
        // Contact
        for (int i = 0; i < _nbFirefly; i++)
        {
            Vector3 pos = _positions[i];

            for (int j = i + 1; j < _nbFirefly; j++)
            {
                if (Vector3.Distance(pos, _positions[j]) <= RADIUS)
                {
                    _neighborsIndex[i * _maxNeighbors + _neighborsCount[i]++] = j;
                    _neighborsIndex[j * _maxNeighbors + _neighborsCount[j]++] = i;
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
                if (Math.Abs(_colorLerpValues[neighborIndex] - 1) < 0.01f)
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
    }
}
