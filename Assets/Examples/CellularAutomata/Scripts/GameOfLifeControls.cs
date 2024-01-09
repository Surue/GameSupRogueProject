using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameOfLifeControls : MonoBehaviour
{
    [Header("Grid")]
    [Range(0, 100)][SerializeField]
    private int _sizeX = 50; [Range(0, 100)][SerializeField] private int _sizeY = 50;

    [Header("Cells")]
    [Range(0, 1)] [SerializeField]
    private float _probabilityIsAlive = 0.5f;

    [Header("Demo")] 
    [Range(0, 10)] [SerializeField]
    private float _stepTime = 0;

    [SerializeField] private bool _s0 = false;
    [SerializeField] private bool _s1 = false;
    [SerializeField] private bool _s2 = false;
    [SerializeField] private bool _s3 = false;
    [SerializeField] private bool _s4 = false;
    [SerializeField] private bool _s5 = false;
    [SerializeField] private bool _s6 = false;
    [SerializeField] private bool _s7 = false;
    [SerializeField] private bool _s8 = false;

    [SerializeField] private bool _b0 = false;
    [SerializeField] private bool _b1 = false;
    [SerializeField] private bool _b2 = false;
    [SerializeField] private bool _b3 = false;
    [SerializeField] private bool _b4 = false;
    [SerializeField] private bool _b5 = false;
    [SerializeField] private bool _b6 = false;
    [SerializeField] private bool _b7 = false;
    [SerializeField] private bool _b8 = false;

    private bool _isRunning = false;

    #region struct

    private struct Cell
    {
        public bool currentState;
        public bool futureState;
    }

    private Cell[,] _cells;
    #endregion

    private List<int> _ruleS;
    private List<int> _ruleB;

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

        SetRules();

        StartCoroutine(Simulate());
    }

    // Update is called once per frame
    public bool IsRunning() {
        return _isRunning;
    }

    private IEnumerator Simulate()
    {
        yield return new WaitForSeconds(_stepTime);
        
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

                    if (_cells[x, y].currentState && _ruleS.Contains(aliveNeighbours)) {
                        _cells[x, y].futureState = true;
                    } else if (!_cells[x, y].currentState && _ruleB.Contains(aliveNeighbours)) {
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

            yield return new WaitForSeconds(_stepTime);
        }
    }

    private void SetRules()
    {
        _ruleB = new List<int>();
        _ruleS = new List<int>();

        if (_b0) {
            _ruleB.Add(0);
        }
        if(_b1) {
            _ruleB.Add(1);
        }
        if(_b2) {
            _ruleB.Add(2);
        }
        if(_b3) {
            _ruleB.Add(3);
        }
        if(_b4) {
            _ruleB.Add(4);
        }
        if(_b5) {
            _ruleB.Add(5);
        }
        if(_b6) {
            _ruleB.Add(6);
        }
        if(_b7) {
            _ruleB.Add(7);
        }
        if(_b8) {
            _ruleB.Add(8);
        }

        if(_s0) {
            _ruleS.Add(0);
        }
        if(_s1) {
            _ruleS.Add(1);
        }
        if(_s2) {
            _ruleS.Add(2);
        }
        if(_s3) {
            _ruleS.Add(3);
        }
        if(_s4) {
            _ruleS.Add(4);
        }
        if(_s5) {
            _ruleS.Add(5);
        }
        if(_s6) {
            _ruleS.Add(6);
        }
        if(_s7) {
            _ruleS.Add(7);
        }
        if(_s8) {
            _ruleS.Add(8);
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
