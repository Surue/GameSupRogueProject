using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BinarySpacePartionning : MonoBehaviour
{
    //Option 
    [Range(0, 50)] [SerializeField] private float _sizeX;
    [Range(0, 50)] [SerializeField] private float _sizeY;

    [Range(0, 50)] [SerializeField] private float _roomSizeX;
    [Range(0, 50)] [SerializeField] private float _roomSizeY;

    [SerializeField] private bool _useSeed = false;

    //Struct
    private struct Room {
        public Vector2 center;
        public Vector2 extends;

        public List<Room> children;
    }

    private Room _rootRoom;

    private void Start()
    {
        Clear();
        Generate();
    }

    public void Generate() {
        _rootRoom.extends = new Vector2(_sizeX * 2, _sizeY * 2);
        _rootRoom.center = Vector2.zero;
        _rootRoom.children = new List<Room>();

        // First snapshot
        SnapshotRecorder.Instance.BeginNewSnapshot("Initial State");
        SnapshotRecorder.Instance.AddSnapshotElement(new SnapshotGizmoWireCube(_rootRoom.center, _rootRoom.extends, Color.white));
        
        _rootRoom.children.AddRange(CheckDivision(_rootRoom));
        
        // Final snapshot
        SnapshotRecorder.Instance.BeginNewSnapshot("Final Result");
        AddExistingRoomToSnapshotAndAllChildren(_rootRoom);
    }

    public void Clear()
    {
        _rootRoom = new Room();
    }

    private List<Room> CheckDivision(Room room) {
        List<Room> childrenList = new List<Room>();
        
        if(room.extends.x > _roomSizeX * 2 && room.extends.y > _roomSizeY * 2) {
            childrenList.AddRange(DivideByProbability(room));
        }else if (room.extends.x > _roomSizeX * 2) {
            childrenList.AddRange(DivideByX(room));
        } else if(room.extends.y > _roomSizeY * 2) {
            childrenList.AddRange(DivideByY(room));
        }

        return childrenList;
    }

    private List<Room> DivideByProbability(Room room) {
        var probability = _useSeed ? RandomSeed.GetValue() : Random.Range(0f, 1f);

        return probability > 0.5 ? DivideByX(room) : DivideByY(room);
    }

    private List<Room> DivideByX(Room room) {
        List<Room> rooms = new List<Room>();

        Room roomLeft;
        Room roomRight;

        //Value for cut
        float posX;

        if (_useSeed)
        {
            posX = RandomSeed.GetValue() * (room.extends.x - _roomSizeX * 0.5f) + _roomSizeX * 0.5f;
        }
        else
        {
            posX = Random.Range(0 + _roomSizeX * 0.5f, room.extends.x - _roomSizeX * 0.5f);
        }

        //Extends
        roomRight.extends = new Vector2(posX, room.extends.y);
        roomLeft.extends = new Vector2(room.extends.x - posX, room.extends.y);

        //Center
        roomRight.center = new Vector2(room.center.x + room.extends.x * 0.5f - roomRight.extends.x * 0.5f, room.center.y);
        roomLeft.center = new Vector2(room.center.x - room.extends.x * 0.5f + roomLeft.extends.x * 0.5f, room.center.y);

        //Children
        roomRight.children = new List<Room>();
        roomLeft.children = new List<Room>();

        //Add children
        room.children.Add(roomRight);
        room.children.Add(roomLeft);
        
        // Snapshot
        TakeSnapshot(room, "Division along X axis");

        // Subdivision
        roomRight.children.AddRange(CheckDivision(roomRight));
        roomLeft.children.AddRange(CheckDivision(roomLeft));
        
        return rooms;
    }

    private List<Room> DivideByY(Room room) {
        List<Room> rooms = new List<Room>();

        Room roomUp;
        Room roomDown;

        //Value for cut
        float posY;

        if (_useSeed)
        {
            posY = RandomSeed.GetValue() * (room.extends.y - _roomSizeY * 0.5f) + _roomSizeY * 0.5f;
        }
        else
        {
            posY = Random.Range(0 + _roomSizeY * 0.5f, room.extends.y - _roomSizeY * 0.5f);
        }

        //Extends
        roomDown.extends = new Vector2(room.extends.x, posY);
        roomUp.extends = new Vector2(room.extends.x, room.extends.y - posY);

        //Center
        roomDown.center = new Vector2(room.center.x, room.center.y - room.extends.y * 0.5f + roomDown.extends.y * 0.5f);
        roomUp.center = new Vector2(room.center.x, room.center.y + room.extends.y * 0.5f - roomUp.extends.y * 0.5f);

        //Children
        roomDown.children = new List<Room>();
        roomUp.children = new List<Room>();

        //Add children
        room.children.Add(roomUp);
        room.children.Add(roomDown);
        
        // Snapshot
        TakeSnapshot(room, "Division along Y axis");

        // Subdivision
        roomDown.children.AddRange(CheckDivision(roomDown));
        roomUp.children.AddRange(CheckDivision(roomUp));

        return rooms;
    }

    private void TakeSnapshot(Room room, string description)
    {
        SnapshotRecorder.Instance.BeginNewSnapshot(description);
        if (room.extends != _rootRoom.extends)
        {
            AddExistingRoomToSnapshotAndAllChildren(_rootRoom);
        }
        SnapshotRecorder.Instance.AddSnapshotElement(new SnapshotGizmoWireCube(room.children[0].center, room.children[0].extends, Color.red));
        SnapshotRecorder.Instance.AddSnapshotElement(new SnapshotGizmoWireCube(room.children[1].center, room.children[1].extends, Color.red));
        SnapshotRecorder.Instance.AddSnapshotElement(new SnapshotGizmoWireCube(room.center, room.extends, Color.blue));
    }
    
    private void AddExistingRoomToSnapshotAndAllChildren(Room room)
    {
        SnapshotRecorder.Instance.AddSnapshotElement(new SnapshotGizmoWireCube(room.center, room.extends, Color.white));
        
        if (room.children == null) return;
        for (int i = 0; i < room.children.Count; i++)
        {
            AddExistingRoomToSnapshotAndAllChildren(room.children[i]);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BinarySpacePartionning))]
public class BspEditor:Editor {
    public override void OnInspectorGUI() {
        BinarySpacePartionning myTarget = (BinarySpacePartionning)target;

        if(GUILayout.Button("Generate")) {
            myTarget.Generate();
        }

        if(GUILayout.Button("Clear")) {
            myTarget.Clear();
        }

        DrawDefaultInspector();
    }
}
#endif