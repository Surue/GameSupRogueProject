using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NormalRandom : MonoBehaviour
{
    [SerializeField] private float _size;
    [SerializeField] private int _nbPoint;
    [SerializeField] private float _mean;
    [SerializeField] private float _dis;

    private List<Vector2> _points;
    private List<Vector2> _points2;

    private bool _isRunning = false;

    // Start is called before the first frame update
    private void Start()
    {
        _points = new List<Vector2>();

        for (int i = 0; i < _nbPoint; i++) {
            Vector2 p = new Vector2(Random.Range(0, _size), Random.Range(0, _size));
            _points.Add(p);
        }

        _points2 = new List<Vector2>();
        for (int i = 0; i < _nbPoint; i++) {
            Vector2 p = new Vector2(RandomNormal(_mean, _dis), RandomNormal(_mean, _dis));
            p = new Vector2(p.x , p.y);
            _points2.Add(p);
        }

        _isRunning = true;
    }

    private void OnValidate()
    {
        _points = new List<Vector2>();

        for (int i = 0; i < _nbPoint; i++) {
            Vector2 p = new Vector2(Random.Range(0, _size), Random.Range(0, _size));
            _points.Add(p);
        }

        _points2 = new List<Vector2>();
        for (int i = 0; i < _nbPoint; i++) {
            Vector2 p = new Vector2(RandomNormal(_mean, _dis), RandomNormal(_mean, _dis));
            p = new Vector2(p.x , p.y);
            _points2.Add(p);
        }

        _isRunning = true;
    }

    // Mu is the mean, sigma is the standard deviation
    private float RandomNormal(float mu, float sigma)
    {
        float a = Random.Range(0.0f, 1.0f);
        float b = Random.Range(0.0f, 1.0f);
        float c = Mathf.Sqrt(-2.0f * Mathf.Log(a)) * Mathf.Cos(2.0f * Mathf.PI * b);

        return mu + (sigma * c);
    }

    private void OnDrawGizmos()
    {
        if (!_isRunning) return;

        foreach (Vector2 v in _points) {
            Gizmos.DrawWireSphere(v, 0.1f);
        }

        Gizmos.color = Color.red;
        foreach(Vector2 v in _points2) {
            Gizmos.DrawWireSphere(v, 0.1f);
        }
    }
}
