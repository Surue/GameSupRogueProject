using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    [Range(0, 1000)][SerializeField] private int _size = 10;
    [Range(0, 100)][SerializeField] private int _iteration = 10;

    private struct Cell {
        public bool isAlive;
        public bool futureState;

        public int region;
    }

    private Cell[,] _cells;

    private bool _isRunning = false;

    private int _currentRegion = 0;

    private List<Color> _colors;

    // Start is called before the first frame update
    private void Start() {
        _cells = new Cell[_size, _size];

        _colors = new List<Color> {
            Color.white,
            Color.blue,
            Color.cyan,
            Color.gray,
            Color.green,
            Color.magenta,
            Color.red,
            Color.yellow
        };
        
        _isRunning = true;

        Generate();
    }

    private void Generate()
    {
       Init();

       StartCoroutine(Cellular());
    }

    private void Init()
    {
        for(int x = 0;x < _size;x++) {
            for(int y = 0;y < _size;y++) {
                _cells[x, y] = new Cell();

                _cells[x, y].region = -1;

                float isAlive = Random.Range(0f, 1f);

                _cells[x, y].isAlive = isAlive < 0.5f;
            }
        }
    }

    private IEnumerator Cellular()
    {
        for (int i = 0; i < _iteration; i++) {
            BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

            for(int x = 0;x < _size;x++) {
                for(int y = 0;y < _size;y++) {
                    int aliveNeighbours = 0;
                    foreach(Vector2Int b in bounds.allPositionsWithin) {
                        if(b.x == 0 && b.y == 0) continue;
                        if(x + b.x < 0 || x + b.x >= _size || y + b.y < 0 || y + b.y >= _size) continue;

                        if(_cells[x + b.x, y + b.y].isAlive) {
                            aliveNeighbours++;
                        }
                    }

                    if(_cells[x, y].isAlive && (aliveNeighbours == 1 || aliveNeighbours >= 4)) {
                        _cells[x, y].futureState = true;
                    } else if(!_cells[x, y].isAlive && aliveNeighbours >= 5) {
                        _cells[x, y].futureState = true;
                    } else {
                        _cells[x, y].futureState = false;
                    }
                }
            }

            for(int x = 0;x < _size;x++) {
                for(int y = 0;y < _size;y++) {
                    _cells[x, y].isAlive= _cells[x, y].futureState;
                }
            }

            yield return null;
        }

        StartCoroutine(GetRoom());
    }

    private IEnumerator GetRoom()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < _size; x++) {
            for (int y = 0; y < _size; y++) {
                if (!_cells[x, y].isAlive) continue;
                if (_cells[x, y].region != -1) continue;

                List<Vector2Int> openList = new List<Vector2Int>();
                List<Vector2Int> closedList = new List<Vector2Int>();

                openList.Add(new Vector2Int(x, y));
                
                while(openList.Count > 0) {
                    _cells[openList[0].x, openList[0].y].region = _currentRegion;
                    closedList.Add(openList[0]);

                    foreach(Vector2Int b in bounds.allPositionsWithin) {
                        //Check not self
                        if(b.x == 0 && b.y == 0) continue;

                        //Check if is on cross
                        if (b.x != 0 && b.y != 0) continue;

                        Vector2Int pos = new Vector2Int(openList[0].x + b.x, openList[0].y + b.y);

                        //Check inside bounds
                        if(pos.x < 0 || pos.x >= _size || pos.y < 0 || pos.y >= _size) continue;

                        //Check is alive
                        if(!_cells[pos.x, pos.y].isAlive) continue;
                        
                        //check region not yet associated
                        if(_cells[pos.x, pos.y].region != -1) continue;

                        //Check if already visited
                        if (closedList.Contains(pos)) continue;

                        //Check if already set to be visited
                        if (openList.Contains(pos)) continue; //Error

                        openList.Add(new Vector2Int(pos.x, pos.y));

                    }
                    openList.RemoveAt(0);
                    
                    yield return null;
                }

                _currentRegion++;

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void OnDrawGizmos() {
        if(!_isRunning) return;

        for(int x = 0;x < _size;x++) {
            for(int y = 0;y < _size;y++) {
                if(_cells[x, y].isAlive) 
                {
                    DrawAliveCell(new Vector2Int(x, y));
                } 
                else 
                {
                    DrawDeadCell(new Vector2(x, y));
                }
            }
        }
    }

    private void DrawAliveCell(Vector2Int pos)
    {
        Gizmos.color = _cells[pos.x, pos.y].region < 0 ? Color.clear : _colors[_cells[pos.x, pos.y].region % _colors.Count];

        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }

    private void DrawDeadCell(Vector2 pos) {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }
}
