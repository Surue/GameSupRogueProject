using UnityEngine;
using System.Collections.Generic;

public class StayInRadiusBehavior : BoidBehavior
{
    [SerializeField] private Vector2 _center = Vector2.zero;
    [SerializeField] private float _radius = 50f;
    [SerializeField] [Range(0, 1)] private float _threshold = 0.9f; 

    public override Vector2 CalculateMove(Boid agent, List<Transform> context, Boid flock)
    {
        Vector2 centerOffset = _center - (Vector2)agent.transform.position;
        float distance = centerOffset.magnitude;
        float ratio = distance / _radius;

        if (ratio < _threshold)
        {
            return Vector2.zero;
        }

        return centerOffset * (ratio * ratio);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_center, _radius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_center, _radius * _threshold);
    }
}