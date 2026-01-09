using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FirefliesManager : MonoBehaviour
{
    [SerializeField] private GameObject _prefabFirefly;
    [SerializeField] private int _nbFirefly;
    [SerializeField] private Vector2 _spawnAreaSize;

    private List<GameObject> _instantiatedFireflies;
    
    private void Start()
    {
        _instantiatedFireflies = new List<GameObject>();

        SetFireflyCount(_nbFirefly);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, _spawnAreaSize);
    }

    public void SetFireflyCount(int newValue)
    {
        if (_instantiatedFireflies.Count < newValue)
        {
            var gap = newValue - _instantiatedFireflies.Count;
            for (int i = 0; i < gap; i++)
            {
                var instance = Instantiate(_prefabFirefly, new Vector3(Random.Range(-_spawnAreaSize.x * 0.5f, _spawnAreaSize.x * 0.5f), Random.Range(-_spawnAreaSize.y * 0.5f, _spawnAreaSize.y * 0.5f), 0), Quaternion.identity);
            
                _instantiatedFireflies.Add(instance);
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
}
