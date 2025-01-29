using System;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    private List<Node> _nodes = new();
    private List<Edge> _edges = new();

    public List<Node> GetNodes() => _nodes;
    public List<Edge> GetEdges() => _edges;
    
    private void Awake()
    {
        _nodes = new()
        {
            new Node { name = "parentNode" },   // 0
            new Node { name = "child1" },       // 1
            new Node { name = "child2" },       // 2
            new Node { name = "subChild1" },    // 3
            new Node { name = "subChild2" },    // 4
            new Node { name = "subChild3" },    // 5
            new Node { name = "subChild4" },    // 6
        };

        _edges = new()
        {
            new Edge() { parentNode = _nodes[0], childNode = _nodes[1] },
            new Edge() { parentNode = _nodes[0], childNode = _nodes[2] },
            new Edge() { parentNode = _nodes[1], childNode = _nodes[3] },
            new Edge() { parentNode = _nodes[1], childNode = _nodes[4] },
            new Edge() { parentNode = _nodes[2], childNode = _nodes[5] },
            new Edge() { parentNode = _nodes[2], childNode = _nodes[6] },
        };
        
        // BFS
        Debug.Log("BFS");
        List<Node> openNodes = new() { _nodes[0] };

        while (openNodes.Count > 0)
        {
            Node node = openNodes[0];
            Debug.Log(node.name);
            openNodes.RemoveAt(0);
            
            foreach (Edge edge in _edges)
            {
                if (edge.parentNode == node)
                {
                    openNodes.Add(edge.childNode);
                }
            }
        }
        
        Debug.Log("DFS");
        Stack<Node> stack = new();
        stack.Push(_nodes[0]);

        while (stack.Count > 0)
        {
            Node node = stack.Pop();
            Debug.Log(node.name);

            foreach (Edge edge in _edges)
            {
                if (edge.parentNode == node)
                {
                    stack.Push(edge.childNode);
                }
            }
        }
    }
}
