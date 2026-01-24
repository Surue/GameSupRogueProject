using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public abstract class BoidBehavior : MonoBehaviour
{
     [SerializeField] protected float _weight = 1f;
     
     public float Weight =>  _weight;

    public abstract Vector2 CalculateMove(Boid agent, List<Transform> context, Boid flock);
}