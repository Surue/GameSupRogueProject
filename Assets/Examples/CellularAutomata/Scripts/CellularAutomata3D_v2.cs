using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata3DV2 : MonoBehaviour {

    [Header("Settings")] 
    [Range(0, 1000)][SerializeField] private int _size = 10;
    [Range(0, 100)][SerializeField] private int _iteration = 10;

    [SerializeField] private GameObject _cubePrefab;

    private struct Cell {
        public int aliveNeighbors;
        public bool isAlive;
        public bool futureState;
        public int[] neighborsIndex;
        public GameObject cube;
    }

    private Cell[] _cells;
    
    // Start is called before the first frame update
    private void Start()
    {
        //Setup base array
        _cells = new Cell[_size * _size * _size];
        
        GenerateGameObjects();

        SetNeighbors();

        RandomBirth();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_iteration <= 0) {
            return;
        }

        foreach (Cell cell in _cells) {
            if (cell.isAlive && cell.aliveNeighbors < 26) {
                cell.cube.gameObject.SetActive(true);
            }
            else {
                cell.cube.gameObject.SetActive(false);
            }
        }

        CellularStep();
        _iteration--;
    }

    private void GenerateGameObjects() {
        int index = 0;
        for (int x = 0; x < _size; x++) {
            for (int y = 0; y < _size; y++) {
                for (int z = 0; z < _size; z++) {
                    _cells[index] = new Cell {cube = Instantiate(_cubePrefab, new Vector3(x, y, z), Quaternion.identity)};

                    index++;
                }
            }
        }
    }

    private void SetNeighbors() {
        BoundsInt bounds = new BoundsInt(-1, -1, -1, 3, 3, 3);
        int index = 0;
        for (int x = 0; x < _size; x++) {
            for (int y = 0; y < _size; y++) {
                for (int z = 0; z < _size; z++) {
                    List<int> tmpNeighbors = new List<int>();
                    
                    foreach (Vector3Int b in bounds.allPositionsWithin) {
                        if (b.x == 0 && b.y == 0 && b.z == 0) continue;
                        if (x + b.x < 0 || x + b.x >= _size) continue;
                        if (y + b.y < 0 || y + b.y >= _size) continue;
                        if (z + b.z < 0 || z + b.z >= _size) continue;
                            
                        tmpNeighbors.Add(CoordToLinearIndex(new Vector3Int(x + b.x, y + b.y, z + b.z)));
                    }
                    
                    _cells[index].neighborsIndex = new int[tmpNeighbors.Count];
                    _cells[index].neighborsIndex = tmpNeighbors.ToArray();
                    index++;
                }
            }
        }
    }

    private void RandomBirth() {
        for (int index = 0; index < _cells.Length; index++) {
            int i = Random.Range(0, 2);

            if (i == 0) {
                _cells[index].isAlive = true;
                foreach (int t in _cells[index].neighborsIndex) {
                    _cells[t].aliveNeighbors++;
                }
            }
            else {
                _cells[index].isAlive = false;
            }
        }
    }

    private void CellularStep() {
        for (int i = 0; i < _cells.Length; i++) {
            //Apply rules
            if (_cells[i].isAlive && (_cells[i].aliveNeighbors >= 13 && _cells[i].aliveNeighbors <= 26)) {
                _cells[i].futureState = true;
            } else if (!_cells[i].isAlive &&
                       ((_cells[i].aliveNeighbors >= 13 && _cells[i].aliveNeighbors <= 14) ||
                        (_cells[i].aliveNeighbors >= 17 && _cells[i].aliveNeighbors <= 19))) {
                _cells[i].futureState = true;
            } else {
                _cells[i].futureState = false;
            }
        }

        for (int i = 0; i < _cells.Length; i++) {
            if (_cells[i].futureState && !_cells[i].isAlive) {
                _cells[i].isAlive = true;

                foreach (int t in _cells[i].neighborsIndex) {
                    _cells[t].aliveNeighbors++;
                }
            }else if (!_cells[i].futureState && _cells[i].isAlive) {
                _cells[i].isAlive = false;

                foreach (int t in _cells[i].neighborsIndex) {
                    _cells[t].aliveNeighbors--;
                }
            }
        }
    }

    private int CoordToLinearIndex(Vector3Int pos) {
        return (pos.x * _size * _size) + (pos.y * _size) + pos.z;
    }
}
