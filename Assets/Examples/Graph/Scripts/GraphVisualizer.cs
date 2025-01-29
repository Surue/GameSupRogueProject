using System.Collections.Generic;
using UnityEngine;

public class GraphVisualizer : MonoBehaviour
{
    [SerializeField] private Graph _graph;
    
    [SerializeField] private GameObject _nodePrefab;

    private List<GameObject> _nodesGo = new();

    private float _speed = 0.1f;
    
    void Start()
    {
        List<Node> list = _graph.GetNodes();
        for (int i = 0; i < list.Count; i++)
        {
            Node node = list[i];
            GameObject nodeGameObject = Instantiate(_nodePrefab, transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0), Quaternion.identity);
            nodeGameObject.name = node.name;
            _nodesGo.Add(nodeGameObject);
        }
    }

    public void Update()
    {
        List<Vector3> newPositions = new(_nodesGo.Count);
        foreach (GameObject go in _nodesGo)
        {
            newPositions.Add(go.transform.position);
        }
        for (int i = 0; i < _nodesGo.Count; i++)
        {
            GameObject nodeGo1 = _nodesGo[i];
            
            
            for (int j = i; j < _nodesGo.Count; j++)
            {
                GameObject nodeGo2 = _nodesGo[j];
                float distance = Vector3.Distance(nodeGo1.transform.position, nodeGo2.transform.position);

                if (distance < 3)
                {
                    Vector3 dir = (nodeGo1.transform.position - nodeGo2.transform.position).normalized;
                    newPositions[i] += dir * _speed * Time.deltaTime;
                    newPositions[j] -= dir * _speed * Time.deltaTime;
                }
            }
        }

        foreach (Edge edge in _graph.GetEdges())
        {
            GameObject nodeGo1 = FindNodeGameObjectByName(edge.parentNode.name);
            GameObject nodeGo2 = FindNodeGameObjectByName(edge.childNode.name);
            
            float distance = Vector3.Distance(nodeGo1.transform.position, nodeGo2.transform.position);
            if (distance < 3)
            {
                Vector3 dir = (nodeGo1.transform.position - nodeGo2.transform.position).normalized;
                newPositions[FindNodeIndexByName(edge.parentNode.name)] += dir * _speed * Time.deltaTime;
                newPositions[FindNodeIndexByName(edge.childNode.name)] -= dir * _speed * Time.deltaTime;
            }else if (distance > 3)
            {
                Vector3 dir = (nodeGo1.transform.position - nodeGo2.transform.position).normalized;
                newPositions[FindNodeIndexByName(edge.parentNode.name)] -= dir * _speed * Time.deltaTime;
                newPositions[FindNodeIndexByName(edge.childNode.name)] += dir * _speed * Time.deltaTime;
            }
        }

        for (int i = 0; i < _nodesGo.Count; i++)
        {
            GameObject o = _nodesGo[i];
            if (o.name.Contains("ParentNode"))
            {
                newPositions[i] = Vector3.zero;
            }

            if (o.name.Contains("subChild"))
            {
                newPositions[i] += Vector3.down * _speed * 2 * Time.deltaTime;
            }
        }

        for (int i = 0; i < _nodesGo.Count; i++)
        {
            GameObject go = _nodesGo[i];
            go.transform.position = newPositions[i];
        }
    }

    private int FindNodeIndexByName(string nodeName)
    {
        for (int i = 0; i < _nodesGo.Count; i++)
        {
            GameObject go = _nodesGo[i];
            if (go.name == nodeName)
            {
                return i;
            }
        }

        return 0;
    }
    
    private GameObject FindNodeGameObjectByName(string nodeName)
    {
        foreach (GameObject go in _nodesGo)
        {
            if (go.name == nodeName)
            {
                return go;
            }
        }
        
        return null;
    }

    void OnDrawGizmos()
    {
        foreach (Edge edge in _graph.GetEdges())
        {
            Vector3 node1Pos = FindNodeGameObjectByName(edge.parentNode.name).transform.position;
            Vector3 node2Pos = FindNodeGameObjectByName(edge.childNode.name).transform.position;
            
            Gizmos.DrawLine(node1Pos, node2Pos);
        }
    }
}
