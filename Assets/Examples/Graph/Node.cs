using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public string name;
    public int distanceToStart;
    public NodeType nodeType;
    
    public enum NodeType
    {
        START_ROOM,
        ENEMY,
        CHEST,
        MERCHANT
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
