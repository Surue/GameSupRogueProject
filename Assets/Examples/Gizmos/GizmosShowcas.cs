using System;
using UnityEngine;

public class GizmosShowcas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down);
    }
}
