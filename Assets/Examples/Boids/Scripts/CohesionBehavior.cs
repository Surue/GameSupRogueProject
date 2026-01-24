using System.Collections.Generic;
using UnityEngine;

public class CohesionBehavior : BoidBehavior
{
    public override Vector2 CalculateMove(Boid agent, List<Transform> context, Boid flock)
    {
        if (context.Count == 0) return Vector2.zero;

        Vector2 cohesionMove = Vector2.zero;
        foreach (Transform item in context)
        {
            cohesionMove += (Vector2)item.position;
        }
        
        cohesionMove /= context.Count;
        cohesionMove -= (Vector2)agent.transform.position;
        return cohesionMove;
    }
}