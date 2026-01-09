using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Firefly : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private CircleCollider2D _circleCollider2D;
    [SerializeField] private float _energyIncreaseOverTime;
    [SerializeField] private float _colorDecreaseOverTime;
    [SerializeField] private float _energyInteraction;
    [SerializeField] private float _neighborRadius;
    
    private List<Firefly> _neighbors = new List<Firefly>();

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

    [SerializeField] private float _acceleration = 0.1f;
    private Vector2 _velocity;
    
    private void Update()
    {
        float friction = 0.999f;
        Vector2 acc = new Vector2(Random.Range(-_acceleration, _acceleration), Random.Range(-_acceleration, _acceleration));
        _velocity = friction * _velocity + acc * Time.deltaTime;
        var transform1 = transform;
        transform1.position += (Vector3)_velocity * Time.deltaTime;

        if (transform1.position.x < -9)
        {
            _velocity.x *= -0.7f;
            transform1.position = new Vector3(-9, transform1.position.y);
        } 
        else if (transform1.position.x > 9)
        {
            _velocity.x *= -0.7f;
            transform1.position = new Vector3(9, transform1.position.y);
        }
        
        if (transform1.position.y < -5)
        {
            _velocity.y *= -0.7f;
            transform1.position = new Vector3(transform1.position.x, -5);
        } 
        else if (transform1.position.y > 5)
        {
            _velocity.y *= -0.7f;
            transform1.position = new Vector3(transform1.position.x, 5);
        }
    }

    private void LateUpdate()
    {
        if (!_isEmittingLight)
        {
            Vector2 selfPosition = transform.position;
            int sum = 0;
            foreach (var neighbor in _neighbors)
            {
                if (Math.Abs(neighbor._colorLerpValue - 1) < 0.01f && Vector2.Distance(selfPosition, neighbor.transform.position) < _neighborRadius)
                {
                    sum++;
                }
            }

            _energy += sum * _energyInteraction;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Firefly neighbor))
        {
            if (!_neighbors.Contains(neighbor))
            {
                _neighbors.Add(neighbor);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Firefly neighbor))
        {
            _neighbors.Remove(neighbor);
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var neighbor in _neighbors)
        {
            Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}
