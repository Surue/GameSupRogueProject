using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BspAdvanced : MonoBehaviour
{
    //Option 
    [Range(0, 50)] [SerializeField] private float _sizeX;
    [Range(0, 50)] [SerializeField] private float _sizeY;

    [Range(0, 50)] [SerializeField] private float _minSizeX;
    [Range(0, 50)] [SerializeField] private float _minSizeY;

    [Range(0, 50)] [SerializeField] private float _maxSizeX;
    [Range(0, 50)] [SerializeField] private float _maxSizeY;

    [Range(0, 1)] [SerializeField] private float _probabilityToCut;
    [Range(0, 1)] [SerializeField] private float _probabilityToByXOrByY;

    //Struct
    private struct Room
    {
        public Vector2 center;
        public Vector2 extends;

        public List<Room> children;
    }

    private Room _rootRoom;


    public void Generate()
    {
        _rootRoom.extends = new Vector2(_sizeX * 2, _sizeY * 2);
        _rootRoom.center = Vector2.zero;
        _rootRoom.children = new List<Room>();

        _rootRoom.children.AddRange(CheckDivision(_rootRoom));
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
        float posX = Random.Range(0 + _minSizeX, room.extends.x - _minSizeX * 2);

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
        float posY = Random.Range(0 + _minSizeY, room.extends.y - (_minSizeY * 2));

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

        roomDown.children.AddRange(CheckDivision(roomDown));
        roomUp.children.AddRange(CheckDivision(roomUp));

        return rooms;
    }

    private void OnDrawGizmos()
    {
        DrawRoom(_rootRoom);
    }

    private static void DrawRoom(Room room)
    {
        Gizmos.DrawWireCube(room.center, room.extends);

        if (room.children == null) return;
        foreach (Room roomChild in room.children) {
            Gizmos.color = Color.cyan;
            DrawRoom(roomChild);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BspAdvanced))]
public class BspAdvancedEditor : Editor
{
    public override void OnInspectorGUI() {
        BspAdvanced myTarget = (BspAdvanced)target;

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
