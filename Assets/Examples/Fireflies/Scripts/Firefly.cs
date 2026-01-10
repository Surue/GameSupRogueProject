using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Firefly : MonoBehaviour
{
    public const float RADIUS = 1.5f;
    public const float ACCELERATION = 0.5f;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private float _energyIncreaseOverTime;
    [SerializeField] private float _colorDecreaseOverTime;
    [SerializeField] private float _energyInteraction;
    [SerializeField] private float _neighborRadius;
    
    public Vector2 _velocity;
    
    public List<Firefly> Neighbors = new List<Firefly>();

    private float _energy;

    private bool _isEmittingLight;

    private float _colorLerpValue;
    
    private void Start()
    {
        _energy = Random.Range(0.0f, 1.0f);
        _isEmittingLight = false;
        _colorLerpValue = 0;
        _sprite.color = Color.Lerp(Color.gray, Color.yellow, _colorLerpValue);
    }

    private void FixedUpdate()
    {
        if (_isEmittingLight)
        {
            _sprite.color = Color.Lerp(Color.gray, Color.yellow, _colorLerpValue);
            _colorLerpValue -= _colorDecreaseOverTime;

            if (_colorLerpValue <= 0)
            {
                _isEmittingLight = false;
            }
        }
        else
        {
            _energy += _energyIncreaseOverTime;

            if (_energy > 1)
            {
                _energy = 0;
                _isEmittingLight = true;
                _colorLerpValue = 1;
            }
        }
    }

    private void LateUpdate()
    {
        if (!_isEmittingLight)
        {
            Vector2 selfPosition = transform.position;
            int sum = 0;
            foreach (var neighbor in Neighbors)
            {
                if (Math.Abs(neighbor._colorLerpValue - 1) < 0.01f && Vector2.Distance(selfPosition, neighbor.transform.position) < _neighborRadius)
                {
                    sum++;
                }
            }

            _energy += sum * _energyInteraction;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        foreach (var neighbor in Neighbors)
        {
            Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}
