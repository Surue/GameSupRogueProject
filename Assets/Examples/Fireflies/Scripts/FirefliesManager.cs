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
    
    [Header("Space partitioning")]
    [SerializeField] private bool _useSpacePartitioning;
    
    private List<Transform> _instantiatedFireflies = new();
    private List<SpriteRenderer> _sprites = new();

    private List<Vector3> _positions = new ();
    private List<Vector2> _velocities = new ();
    private List<float> _energies = new ();
    private List<float> _colorLerpValues = new ();
    private List<bool> _isEmittingLights = new ();
    
    private List<List<int>> _neighborsIndex;
    
    private void Start()
    {
        _instantiatedFireflies = new List<Transform>();
        _neighborsIndex = new List<List<int>>();
        _energies = new List<float>();
        _colorLerpValues = new List<float>();
        _isEmittingLights = new List<bool>();

        SetFireflyCount(_nbFirefly);
    }

    private void FixedUpdate()
    {
        if (_instantiatedFireflies.Count == 0)
        {
            return;
        }
        
        int fireflyCount = _instantiatedFireflies.Count;
        
        // Reset
        for (int i = 0; i < fireflyCount; i++)
        {
            _neighborsIndex[i].Clear();
        }
        
        // Position
        float friction = 0.999f;
        for (int i = 0; i < fireflyCount; i++)
        {
            Vector2 vel = _velocities[i];
            Vector3 pos = _positions[i];
            Vector2 acc = new Vector2(Random.Range(-ACCELERATION, ACCELERATION), Random.Range(-ACCELERATION, ACCELERATION));
            vel = friction * vel + acc * Time.fixedDeltaTime;
            pos += (Vector3)vel * Time.fixedDeltaTime;

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

        // Contact
        for (int i = 0; i < fireflyCount; i++)
        {
            Vector3 pos = _positions[i];
            List<int> neighbors = _neighborsIndex[i];
            
            for (int j = i + 1; j < fireflyCount; j++)
            {
                if (Vector3.Distance(pos, _positions[j]) <= RADIUS)
                {
                    neighbors.Add(j);
                    _neighborsIndex[j].Add(i);
                }
            }
        }
        
        // Energy
        for (int i = 0; i < fireflyCount; i++)
        {
            if (_isEmittingLights[i]) continue;
            
            int sum = 0;
            var neighbors = _neighborsIndex[i];
            int neighborCount = neighbors.Count;
            for (int j = 0; j < neighborCount; j++)
            {
                int neighborIndex = neighbors[j];
                if (Math.Abs(_colorLerpValues[neighborIndex] - 1) < 0.01f)
                {
                    sum++;
                }
            }

            _energies[i] += sum * ENERGY_INTERACTION;
        }
        
        // Lights
        for (int i = 0; i < fireflyCount; i++)
        {
            if (_isEmittingLights[i])
            {
                _sprites[i].color = Color.Lerp(Color.gray, Color.yellow, _colorLerpValues[i]);
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

    public void SetFireflyCount(int newValue)
    {
        if (_instantiatedFireflies.Count < newValue)
        {
            var gap = newValue - _instantiatedFireflies.Count;
            for (int i = 0; i < gap; i++)
            {
                var instance = Instantiate(_prefabFirefly, new Vector3(Random.Range(-_spawnAreaSize.x * 0.5f, _spawnAreaSize.x * 0.5f), Random.Range(-_spawnAreaSize.y * 0.5f, _spawnAreaSize.y * 0.5f), 0), Quaternion.identity);
            
                _instantiatedFireflies.Add(instance.transform);
                _neighborsIndex.Add(new List<int>());
                _positions.Add(instance.transform.position);
                _velocities.Add(Vector2.zero);
                _energies.Add(Random.Range(0.0f, 1.0f));
                _isEmittingLights.Add(false);
                _colorLerpValues.Add(0);
                _sprites.Add(instance.GetComponentInChildren<SpriteRenderer>());
                _sprites[^1].color = Color.Lerp(Color.gray, Color.yellow, 0);
            }
        }
        else
        {
            var gap = _instantiatedFireflies.Count - newValue;
            for (int i = 0; i < gap; i++)
            {
                Destroy(_instantiatedFireflies[^1]);
                _instantiatedFireflies.RemoveAt(_instantiatedFireflies.Count - 1);
                _positions.RemoveAt(_instantiatedFireflies.Count - 1);
                _velocities.RemoveAt(_instantiatedFireflies.Count - 1);
                _energies.RemoveAt(_instantiatedFireflies.Count - 1);
                _isEmittingLights.RemoveAt(_instantiatedFireflies.Count - 1);
                _colorLerpValues.RemoveAt(_instantiatedFireflies.Count - 1);
                _sprites.RemoveAt(_instantiatedFireflies.Count - 1);
            }
        }
    }

    public int GetFireflyCount()
    {
        return _nbFirefly;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, _spawnAreaSize);

        for (var i = 0; i < _instantiatedFireflies.Count; i++)
        {
            var firefly = _instantiatedFireflies[i];
            if (Selection.activeGameObject == firefly.gameObject)
            {
                foreach (var neighborIndex in _neighborsIndex[i])
                {
                    Gizmos.DrawLine(firefly.transform.position, _instantiatedFireflies[neighborIndex].transform.position);
                }
            }
        }
    }
}
