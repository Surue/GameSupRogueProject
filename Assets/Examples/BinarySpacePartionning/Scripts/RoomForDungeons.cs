using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "Scriptable Objects/BSP/Room")]
public class RoomForDungeons : ScriptableObject
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Vector2 _roomSize;
    
    public GameObject Prefab => _prefab;
    public Vector2 RoomSize => _roomSize;
}
