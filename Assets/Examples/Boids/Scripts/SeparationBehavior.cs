using System.Collections.Generic;
using UnityEngine;

public class SeparationBehavior : BoidBehavior
{
    [SerializeField] private float _avoidRadius = 2f;

    public override Vector2 CalculateMove(Boid agent, List<Transform> context, Boid flock)
    {
        if (context.Count == 0) return Vector2.zero;

        Vector2 separationMove = Vector2.zero;
        int nAvoid = 0;

        foreach (Transform item in context)
        {
            if (Vector2.Distance(item.position, agent.transform.position) < _avoidRadius)
            {
                nAvoid++;
                separationMove += (Vector2)(agent.transform.position - item.position);
            }
        }

        if (nAvoid > 0)
        {
            separationMove /= nAvoid;
        }
        return separationMove;
    }
}