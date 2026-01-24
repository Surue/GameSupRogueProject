using System;
using UnityEngine;
using System.Collections.Generic;

public class Boid : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _neighborRadius = 5f; 

    private Vector2 _velocity;
    private List<BoidBehavior> _behaviors = new List<BoidBehavior>();
    private List<Vector2> _debugDir = new List<Vector2>();

    private void Start()
    {
        _behaviors.AddRange(GetComponents<BoidBehavior>());
        _velocity = transform.up * _speed;
        
        _debugDir = new List<Vector2>(_behaviors.Count);
        for (int i = 0; i < _behaviors.Count; i++)
        {
            _debugDir.Add(Vector2.zero);
        }
    }
    
    private void Update()
    {
        List<Transform> context = GetNearbyObjects();
        Vector2 move = Vector2.zero;

        for (var i = 0; i < _behaviors.Count; i++)
        {
            var behavior = _behaviors[i];
            Vector2 partialMove = behavior.CalculateMove(this, context, this) * behavior.Weight;
            _debugDir[i] = partialMove;

            if (partialMove != Vector2.zero)
            {
                if (partialMove.sqrMagnitude > behavior.Weight * behavior.Weight)
                {
                    partialMove.Normalize();
                    partialMove *= behavior.Weight;
                }

                move += partialMove;
            }
        }

        if (move != Vector2.zero)
        {
            _velocity = Vector2.Lerp(_velocity, move, Time.deltaTime * 2f);
        }

        _velocity = _velocity.normalized * _speed;
        transform.position += (Vector3)_velocity * Time.deltaTime;
        
        transform.up = _velocity;
    }

    private List<Transform> GetNearbyObjects()
    {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(transform.position, _neighborRadius);
        foreach (Collider2D c in contextColliders)
        {
            if (c.gameObject != gameObject) context.Add(c.transform);
        }
        return context;
    }

    private void OnDrawGizmosSelected()
    {
        for (var i = 0; i < _behaviors.Count; i++)
        {
            Gizmos.DrawRay(transform.position, _debugDir[i]);
        }
    }
}