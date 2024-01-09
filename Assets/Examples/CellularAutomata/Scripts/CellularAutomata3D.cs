using System.Collections;
using UnityEngine;

public class CellularAutomata3D : MonoBehaviour
{

    [Range(0, 1000)][SerializeField] private int _size = 10;
    [Range(0, 100)][SerializeField] private int _iteration = 10;

    [SerializeField] private GameObject _cubePrefab;

    private struct Cell
    {
        public bool isAlive;
        public bool futureState;
    }

    private Cell[,,] _cells;

    private bool _isRunning = false;

    // Start is called before the first frame update
    private void Start()
    {
        //Create array
        _cells = new Cell[_size, _size, _size];

        //Fill array by random
        for (int x = 0; x < _size; x++) {
            for (int y = 0; y < _size; y++) {
                for (int z = 0; z < _size; z++) {
                    float f = Random.Range(0f, 1f);
                    _cells[x, y, z].isAlive = f > 0.5f;
                }
            }
        }
        
        StartCoroutine(WorldGeneration());
    }

    private IEnumerator WorldGeneration()
    {
        _isRunning = true;
        //Cellular automata
        for (int i = 0; i < _iteration; i++) {
            Cellular();
            yield return null;
        }

        _isRunning = false;

        //Cut cube
        CutCube();

        //Generate cube
        GenerateCube();
    }

    private void Cellular()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, -1, 3, 3, 3);

        for (int x = 0; x < _size; x++) {
            for (int y = 0; y < _size; y++) {
                for (int z = 0; z < _size; z++) {

                    int neighboursAlive = 0;

                    //Check neighbours
                    foreach (Vector3Int b in bounds.allPositionsWithin) {
                        if (b.x == 0 && b.y == 0 && b.z == 0) continue;
                        if (x + b.x < 0 || x + b.x >= _size) continue;
                        if (y + b.y < 0 || y + b.y >= _size) continue;
                        if (z + b.z < 0 || z + b.z >= _size) continue;

                        if (_cells[x + b.x, y + b.y, z + b.z].isAlive) {
                            neighboursAlive++;
                        }
                    }

                    //Apply rules
                    if (_cells[x, y, z].isAlive && (neighboursAlive >= 13 && neighboursAlive <= 26)) {
                        _cells[x, y, z].futureState = true;
                    } else if (!_cells[x, y, z].isAlive &&
                               ((neighboursAlive >= 13 && neighboursAlive <= 14) ||
                                (neighboursAlive >= 17 && neighboursAlive <= 19))) {
                        _cells[x, y, z].futureState = true;
                    } else {
                        _cells[x, y, z].futureState = false;
                    }
                }
            }
        }

        for (int x = 0; x < _size; x++) {
            for (int y = 0; y < _size; y++) {
                for (int z = 0; z < _size; z++) {
                    _cells[x, y, z].isAlive = _cells[x, y, z].futureState;
                }
            }
        }
    }

    private void CutCube()
    {
        for(int x = 0; x < _size; x++) {
            for (int y = _size - 10; y < _size; y++) {
                for (int z = 0; z < _size; z++) {
                    _cells[x, y, z].isAlive = true;
                }
            }
        }
    }

    private void GenerateCube()
    {
        for(int x = 0;x < _size;x++) {
            for(int y = 0;y < _size;y++) {
                for(int z = 0;z < _size;z++) {
                    if (!_cells[x, y, z].isAlive) {
                        continue;
                    }
                    
                    GameObject instance = Instantiate(_cubePrefab);

                    instance.transform.position = new Vector3(x, y, z);
                }
            }
        }
    }
}
