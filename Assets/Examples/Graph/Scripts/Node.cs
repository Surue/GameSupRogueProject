using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
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
}











public class NodeWithChildren
{
    public string name;
    
    public Node child0;
    public Node child1;
    public bool isChild0Locked;
    public bool isChild1Locked;
}
