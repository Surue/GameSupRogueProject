using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AdvancedBinarySpacePartitionning : MonoBehaviour
{
    #region Nested Types
    private struct Room
    {
        public Vector2 center;
        public Vector2 extends;

        public List<Room> children;
    }
    #endregion
    
    #region Fields
    [Range(0, 50)] [SerializeField] private float _sizeX;
    [Range(0, 50)] [SerializeField] private float _sizeY;

    [Range(0, 50)] [SerializeField] private float _minSizeX;
    [Range(0, 50)] [SerializeField] private float _minSizeY;

    [Range(0, 50)] [SerializeField] private float _maxSizeX;
    [Range(0, 50)] [SerializeField] private float _maxSizeY;

    [Range(0, 1)] [SerializeField] private float _probabilityToCut;
    [Range(0, 1)] [SerializeField] private float _probabilityToByXOrByY;
    
    private Room _rootRoom;
    #endregion
    
    #region Methods
    private void Start()
    {
        Clear();
        Generate();
    }

    public void Generate()
    {
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

    public void Clear() {
        _rootRoom = new Room();
    }

    private List<Room> CheckDivision(Room room)
    {
        List<Room> childrenList = new List<Room>();

        //Divide by max size X
        if (room.extends.x > _maxSizeX || room.extends.y > _maxSizeY) {
            if (room.extends.x > _maxSizeX && room.extends.y > _maxSizeY) {
                childrenList.AddRange(DivideByProbability(room));
            }else if (room.extends.x > _maxSizeX) {
                childrenList.AddRange(DivideByX(room));
            } else {
                childrenList.AddRange(DivideByY(room));
            }
        }
        else if (room.extends.x > _minSizeX * 2 || room.extends.y > _minSizeY * 2) { //Divide by probability
            if(room.extends.x > _minSizeX * 2 && room.extends.y > _minSizeY * 2) {
                float probability = Random.Range(0f, 1f);

                if (probability > _probabilityToCut) {
                    childrenList.AddRange(DivideByProbability(room));
                }
            } else if(room.extends.x > _minSizeX * 2) {
                float probability = Random.Range(0f, 1f);

                if (probability > _probabilityToCut) {
                    childrenList.AddRange(DivideByX(room));
                }
            } else {
                float probability = Random.Range(0f, 1f);

                if(probability > _probabilityToCut) {
                    childrenList.AddRange(DivideByY(room));
                }
            }
        }

        return childrenList;
    }

    private List<Room> DivideByProbability(Room room)
    {
        float probability = Random.Range(0f, 1f);

        return probability > _probabilityToByXOrByY ? DivideByX(room) : DivideByY(room);
    }

    private List<Room> DivideByX(Room room)
    {
        List<Room> rooms = new List<Room>();

        Room roomLeft;
        Room roomRight;

        //Value for cut
        float posX = Random.Range(_minSizeX, room.extends.x - _minSizeX);

        //Extends
        roomRight.extends = new Vector2(posX, room.extends.y);
        roomLeft.extends = new Vector2(room.extends.x - posX, room.extends.y);

        //Center
        float offset = (room.extends.x / 2.0f) - (roomLeft.extends.x / 2.0f);
        roomLeft.center = new Vector2(room.center.x - offset, room.center.y);
        offset = (room.extends.x / 2.0f) - (roomRight.extends.x / 2.0f);
        roomRight.center = new Vector2(room.center.x + offset, room.center.y);

        //Children
        roomRight.children = new List<Room>();
        roomLeft.children = new List<Room>();

        //Add children
        room.children.Add(roomRight);
        room.children.Add(roomLeft);
        
        // Snapshot
        TakeSnapshot(room, "Division along X axis");

        roomRight.children.AddRange(CheckDivision(roomRight));
        roomLeft.children.AddRange(CheckDivision(roomLeft));

        return rooms;
    }

    private List<Room> DivideByY(Room room)
    {
        List<Room> rooms = new List<Room>();

        Room roomUp;
        Room roomDown;

        //Value for cut
        float posY = Random.Range(_minSizeY, room.extends.y - _minSizeY);

        //Extends
        roomDown.extends = new Vector2(room.extends.x, posY);
        roomUp.extends = new Vector2(room.extends.x, room.extends.y - posY);

        //Center
        float offset = (room.extends.y / 2.0f) - (roomDown.extends.y / 2.0f);
        roomDown.center = new Vector2(room.center.x, room.center.y - offset);
        offset = (room.extends.y / 2.0f) - (roomUp.extends.y / 2.0f);
        roomUp.center = new Vector2(room.center.x, room.center.y + offset);

        //Children
        roomDown.children = new List<Room>();
        roomUp.children = new List<Room>();

        //Add children
        room.children.Add(roomUp);
        room.children.Add(roomDown);
        
        // Snapshot
        TakeSnapshot(room, "Division along Y axis");

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
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(AdvancedBinarySpacePartitionning))]
public class BspAdvancedEditor : Editor
{
    public override void OnInspectorGUI() {
        AdvancedBinarySpacePartitionning myTarget = (AdvancedBinarySpacePartitionning)target;

        if (GUILayout.Button("Generate")) {
            myTarget.Generate();
        }

        if(GUILayout.Button("Clear")) {
            myTarget.Clear();
        }

        DrawDefaultInspector();
    }
}
#endif
