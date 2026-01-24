using System.Collections.Generic;
using UnityEngine;

public class AlignmentBehavior : BoidBehavior
{
    public override Vector2 CalculateMove(Boid agent, List<Transform> context, Boid flock)
    {
        if (context.Count == 0) return agent.transform.up;

        Vector2 alignmentMove = Vector2.zero;
        foreach (Transform item in context)
        {
            alignmentMove += (Vector2)item.up;
        }
        
        alignmentMove /= context.Count;
        return alignmentMove;
    }
}