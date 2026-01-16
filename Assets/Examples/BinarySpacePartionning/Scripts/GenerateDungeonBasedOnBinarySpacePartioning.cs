using System.Collections.Generic;
using UnityEngine;

public class GenerateDungeonBasedOnBinarySpacePartioning : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AdvancedBinarySpacePartitionning _bspLogic;
    
    [Header("Prefabs")]
    [SerializeField] private List<GameObject> _roomPrefabs;
    [SerializeField] private List<GameObject> _corridorPrefabs;

    [Header("Settings")]
    [SerializeField] private Transform _container;

    private void Start()
    {
        _bspLogic.Init(true);
        ClearDungeon();
        ProcessRoom(_bspLogic.Root);
    }

    private void ProcessRoom(AdvancedBinarySpacePartitionning.Room room)
    {
        if (room.children == null || room.children.Count == 0)
        {
            InstantiateRoom(room);
        }
        else
        {
            foreach (var child in room.children)
            {
                ProcessRoom(child);
            }

            if (room.children.Count == 2)
            {
                CreateCorridor(room.children[0], room.children[1]);
            }
        }
    }

    private void InstantiateRoom(AdvancedBinarySpacePartitionning.Room room)
    {
        if (_roomPrefabs.Count == 0) return;

        GameObject prefab = _roomPrefabs[Random.Range(0, _roomPrefabs.Count)];
        GameObject instance = Instantiate(prefab, new Vector3(room.center.x, room.center.y, 0), Quaternion.identity, _container);
    }

    private void CreateCorridor(AdvancedBinarySpacePartitionning.Room roomA, AdvancedBinarySpacePartitionning.Room roomB)
    {
        if (_corridorPrefabs.Count == 0) return;

        Vector2 start = roomA.center;
        Vector2 end = roomB.center;
        
        float xDir = Mathf.Sign(end.x - start.x);
        for (float x = start.x; xDir > 0 ? x <= end.x : x >= end.x; x += xDir)
        {
            PlaceCorridorTile(new Vector3(x, start.y, 0));
        }

        float yDir = Mathf.Sign(end.y - start.y);
        for (float y = start.y; yDir > 0 ? y <= end.y : y >= end.y; y += yDir)
        {
            PlaceCorridorTile(new Vector3(end.x, y, 0));
        }
    }

    private void PlaceCorridorTile(Vector3 position)
    {
        GameObject prefab = _corridorPrefabs[Random.Range(0, _corridorPrefabs.Count)];
        Instantiate(prefab, position, Quaternion.identity, _container);
    }

    private void ClearDungeon()
    {
        if (_container == null) return;
        
        foreach (Transform child in _container)
        {
            Destroy(child.gameObject);
        }
    }
}
