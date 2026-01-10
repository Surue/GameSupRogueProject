using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FirefliesManager : MonoBehaviour
{
    [Header("Fireflies")]
    [SerializeField] private GameObject _prefabFirefly;
    [SerializeField] private int _nbFirefly;
    [SerializeField] private Vector2 _spawnAreaSize;
    
    [Header("Space partitioning")]
    [SerializeField] private bool _useSpacePartitioning;
    
    private List<Firefly> _instantiatedFireflies;
    
    private void Start()
    {
        _instantiatedFireflies = new List<Firefly>();

        SetFireflyCount(_nbFirefly);
    }

    private void FixedUpdate()
    {
        if (_instantiatedFireflies.Count == 0)
        {
            return;
        }
        
        float colliderRadius = Firefly.RADIUS;
        float acceleration = Firefly.ACCELERATION;
        
        int fireflyCount = _instantiatedFireflies.Count;
        
        // Reset
        List<Vector3> position = new List<Vector3>(fireflyCount);
        List<Vector2> velocity = new List<Vector2>(fireflyCount);
        for (int i = 0; i < fireflyCount; i++)
        {
            _instantiatedFireflies[i].Neighbors.Clear();
            position.Add(_instantiatedFireflies[i].transform.position);
            velocity.Add(_instantiatedFireflies[i]._velocity);
        }
        
        // Position
        float friction = 0.999f;
        for (int i = 0; i < fireflyCount; i++)
        {
            Vector2 vel =  velocity[i];
            Vector2 acc = new Vector2(Random.Range(-acceleration, acceleration), Random.Range(-acceleration, acceleration));
            vel = friction * vel + acc * Time.fixedDeltaTime;
            position[i] += (Vector3)vel * Time.fixedDeltaTime;

            if (position[i].x < -9)
            {
                vel.x *= -0.7f;
                position[i] = new Vector3(-9, position[i].y);
            }
            else if (position[i].x > 9)
            {
                vel.x *= -0.7f;
                position[i] = new Vector3(9, position[i].y);
            }

            if (position[i].y < -5)
            {
                vel.y *= -0.7f;
                position[i] = new Vector3(position[i].x, -5);
            }
            else if (position[i].y > 5)
            {
                vel.y *= -0.7f;
                position[i] = new Vector3(position[i].x, 5);
            }

            velocity[i] = vel;
        }
        
        for (int i = 0; i < fireflyCount; i++)
        {
            _instantiatedFireflies[i].transform.position = position[i];
            _instantiatedFireflies[i]._velocity = velocity[i];
        }

        // Contact
        for (int i = 0; i < fireflyCount; i++)
        {
            Firefly firstFirefly = _instantiatedFireflies[i];
            Vector3 pos1 = position[i];
            
            for (int j = i + 1; j < fireflyCount; j++)
            {
                Firefly secondFirefly = _instantiatedFireflies[j];
                Vector3 pos2 = position[j];
                
                float distance = Vector3.Distance(pos1, pos2);

                if (distance <= colliderRadius)
                {
                    firstFirefly.Neighbors.Add(secondFirefly);
                    secondFirefly.Neighbors.Add(firstFirefly);
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
            
                _instantiatedFireflies.Add(instance.GetComponent<Firefly>());
            }
        }
        else
        {
            var gap = _instantiatedFireflies.Count - newValue;
            for (int i = 0; i < gap; i++)
            {
                Destroy(_instantiatedFireflies[^1]);
                _instantiatedFireflies.RemoveAt(_instantiatedFireflies.Count - 1);
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
    }
}
