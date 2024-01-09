using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameOfLife : MonoBehaviour
{
    [Header("Grid")]
    [Range(0, 100)][SerializeField] private int _sizeX = 50;
    [Range(0, 100)][SerializeField] private int _sizeY = 50;

    [Header("Cells")]
    [Range(0, 1)] [SerializeField] private float _probabilityIsAlive = 0.5f;

    private bool _isRunning = false;

    #region struct

    private struct Cell
    {
        public bool currentState;
        public bool futureState;
    }

    private Cell[,] _cells;
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        _cells = new Cell[_sizeX, _sizeY];
        for (int x = 0; x < _sizeX; x++) {
            for (int y = 0; y < _sizeY; y++) {
                _cells[x, y] = new Cell();

                float isAlive = Random.Range(0f, 1f);

                _cells[x, y].currentState = isAlive < _probabilityIsAlive;
            }
        }

        _isRunning = true;

        StartCoroutine(Simulate());
    }
    
    private IEnumerator Simulate()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        while (true) {

            for (int x = 0; x < _sizeX; x++) {
                for (int y = 0; y < _sizeY; y++) {
                    int aliveNeighbours = 0;
                    foreach (Vector2Int b in bounds.allPositionsWithin) {
                        if (b.x == 0 && b.y == 0) continue;
                        if (x + b.x < 0 || x + b.x >= _sizeX || y + b.y < 0 || y + b.y >= _sizeY) continue;

                        if (_cells[x + b.x, y + b.y].currentState) {
                            aliveNeighbours++;
                        }
                    }

                    if (_cells[x, y].currentState && (aliveNeighbours == 2 || aliveNeighbours == 3)) {
                        _cells[x, y].futureState = true;
                    } else if (!_cells[x, y].currentState && aliveNeighbours == 3) {
                        _cells[x, y].futureState = true;
                    } else {
                        _cells[x, y].futureState = false;
                    }
                }
            }

            for (int x = 0; x < _sizeX; x++) {
                for (int y = 0; y < _sizeY; y++) {
                    _cells[x, y].currentState = _cells[x, y].futureState;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!_isRunning) return;

        for(int x = 0;x < _sizeX;x++) {
            for(int y = 0;y < _sizeY;y++) {
                if (_cells[x, y].currentState) {
                    DrawAliveCell(new Vector2(x, y));
                } else {
                    DrawDeadCell(new Vector2(x, y));
                }
            }
        }
    }

    private void DrawAliveCell(Vector2 pos)
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }

    private void DrawDeadCell(Vector2 pos)
    {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }
}
