using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public Vector2 Size;
    public Vector2 Center;
}

public class BSP : MonoBehaviour
{
    [SerializeField] private float _sizeX; // Size of the area on the x axis
    [SerializeField] private float _sizeY; // Size of the area on the z axis (this axis are on the X and Z axis)
    
    // Taille minimum
    [SerializeField] private float _minSizeX;
    [SerializeField] private float _minSizeY;
    
    // Chunks
    [SerializeField] private List<GameObject> _chunkPrefab5x5;
    [SerializeField] private List<GameObject> _chunkPrefab10x5;
    [SerializeField] private List<GameObject> _chunkPrefab10x10;
    [SerializeField] private List<GameObject> _chunkPrefab20x20;
    
    // Rooms 
    private List<Room> _rooms; // Declaration
    private List<Room> _cuttableRooms;
    
    
    private void Start()
    {
        // Create first room
        Room rootRoom; // Declaration
        rootRoom = new Room(); // Initialization
        rootRoom.Size = new Vector2(_sizeX, _sizeY);
        rootRoom.Center = new Vector2(0, 0);

        // Add room to list
        _rooms = new List<Room>(); // Initialization
        
        _cuttableRooms = new List<Room>();
        _cuttableRooms.Add(rootRoom);
        
        // Algo
        while (_cuttableRooms.Count > 0)
        {
            // Tant que je peux couper une room en 2, je le fais
            // Si je coupe en 2 je crée des nouvelles rooms
            Room roomToCut = _cuttableRooms[0];
            _cuttableRooms.RemoveAt(0);

            if (roomToCut.Size.x / 2.0f <= _minSizeX || roomToCut.Size.y / 2.0f <= _minSizeY) // || is "or"
            {
                _rooms.Add(roomToCut);
                continue;
            }
            
            // If code reach this point => Cut the room
            bool doCutVertical; // Declaration
            int random =  Random.Range(0, 2); // Can be equal 0 or 1
            doCutVertical = random == 0; // If random == 0 => true, or else

            if (doCutVertical)
            {
                // Cut Vertical
                Room roomLeft = new Room();
                Room roomRight = new Room();

                // Size
                float newWidth = Random.Range(_minSizeX, roomToCut.Size.x - _minSizeX);

                roomLeft.Size = new Vector2(newWidth, roomToCut.Size.y);
                roomRight.Size = new Vector2(roomToCut.Size.x - newWidth, roomToCut.Size.y);
                
                // Center
                float offset = (roomToCut.Size.x / 2.0f) - (roomLeft.Size.x / 2.0f);
                roomLeft.Center = new Vector2(roomToCut.Center.x - offset, roomToCut.Center.y);
                
                offset = (roomToCut.Size.x / 2.0f) - (roomRight.Size.x / 2.0f);
                roomRight.Center = new Vector2(roomToCut.Center.x + offset, roomToCut.Center.y);
                
                // Add room to cuttable rooms
                _cuttableRooms.Add(roomLeft);
                _cuttableRooms.Add(roomRight);
            }
            else
            {
                // Cut Horizontal
                Room roomTop = new Room();
                Room roomBottom = new Room();

                float newHeight = Random.Range(_minSizeY, roomToCut.Size.y - _minSizeY);

                roomTop.Size = new Vector2(roomToCut.Size.x, newHeight);
                roomBottom.Size = new Vector2(roomToCut.Size.x, roomToCut.Size.y - newHeight);
                
                float offset = (roomToCut.Size.y / 2.0f) - (roomTop.Size.y / 2.0f);
                roomTop.Center = new Vector2(roomToCut.Center.x, roomToCut.Center.y - offset);
                
                offset = (roomToCut.Size.y / 2.0f) - (roomBottom.Size.y / 2.0f); 
                roomBottom.Center = new Vector2(roomToCut.Center.x, roomToCut.Center.y + offset);
                
                // Add room to cuttable rooms
                _cuttableRooms.Add(roomTop);
                _cuttableRooms.Add(roomBottom);
            }
        }
        
        // Spawn chunks
        foreach (Room room in _rooms)
        {
            // List all chunks possible to use
            List<GameObject> chunksPossibleToInstantiate = new List<GameObject>();
            if (room.Size.x >= 20 && room.Size.y >= 20)
            {
                // Chunks 20 x 20
                chunksPossibleToInstantiate.AddRange(_chunkPrefab20x20);
            }
            
            if (room.Size.x >= 10) 
            {
                if (room.Size.y >= 10)
                {
                    // Chunks 10 x 10
                    chunksPossibleToInstantiate.AddRange(_chunkPrefab10x10);
                }
                else if (room.Size.y >= 5)
                {
                    // Chunks 10 x 5
                    chunksPossibleToInstantiate.AddRange(_chunkPrefab10x5);
                }
            }
            
            if (room.Size.x >= 5 && room.Size.y >= 5)
            {
                // Chunks 5 x 5
                chunksPossibleToInstantiate.AddRange(_chunkPrefab5x5);
            }
            
            GameObject chunkPrefab = chunksPossibleToInstantiate[Random.Range(0, chunksPossibleToInstantiate.Count)];
            GameObject chunkInstance = Instantiate(chunkPrefab, new Vector3(room.Center.x, 0, room.Center.y), Quaternion.identity);

        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Function to draw gizmos, this function is automatically called from Unity.
    /// </summary>
    private void OnDrawGizmos()
    {
        // This function allows to draw a wire cube on the screen.
        Gizmos.DrawWireCube(transform.position, new Vector3(_sizeX, 0, _sizeY));
        /*
         * Gizmos is special. This is not a object nor a MonoBehavior. This is a collection
         * of static function. All those function must always be called from inside a OnDrawGizmos()
         * or a OnDrawGizmosSelected.
         */

        if (_rooms != null)
        {
            foreach (Room room in _rooms)
            {
                Gizmos.DrawWireCube(new Vector3(room.Center.x, 0, room.Center.y), new Vector3(room.Size.x, 0, room.Size.y));
            }
        }
    }
}
