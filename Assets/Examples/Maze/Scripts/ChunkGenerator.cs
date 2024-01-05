using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    [SerializeField] private int _cellNbX = 2;
    [SerializeField] private int _cellNbY = 7;

    private int _sizeX = 5;
    private int _sizeY = 5;

    struct Cell
    {
        public int x;
        public int y;
        public Chunck chunk;
    }

    [SerializeField] private GameObject[] _chunksAvailable;

    private Cell[,] _cells;

    enum RuleState
    {
        ALWAYS_TRUE,
        ALWAYS_FALSE,
        DONT_CARE
    }

    struct Rule
    {
        public RuleState up;
        public RuleState down;
        public RuleState left;
        public RuleState right;
    }

    private void Start()
    {
        Generate();
    }

    private void Generate()
    {
        if (_sizeX > 0 & _sizeY > 0) {
            _cells = new Cell[_cellNbX, _cellNbY];

            for (int x = 0; x < _cellNbX; x++) {
                for (int y = 0; y < _cellNbY; y++) {
                    _cells[x, y].x = x;
                    _cells[x, y].y = y;
                }
            }

            GenerateStartChunk();
        }
    }

    private void GenerateStartChunk()
    {
        List<GameObject> possibleNewChunk = new ();

        foreach (var chunkAvailable in _chunksAvailable)
        {
            Chunck currentChunk = chunkAvailable.GetComponent<Chunck>();

            if ((currentChunk.right || currentChunk.up) && !currentChunk.left && !currentChunk.down) {
                possibleNewChunk.Add(chunkAvailable);
            }
        }

        var newChunk = possibleNewChunk[Random.Range(0, possibleNewChunk.Count)];

        GameObject startChunk = Instantiate(newChunk);
        startChunk.name = "StartChunk";
        startChunk.transform.position = new Vector2(0 + _sizeX / 2f, 0 + _sizeY / 2f);

        _cells[0, 0].chunk = startChunk.GetComponent<Chunck>();

        GenerateChunkChild(_cells[0, 0]);
    }

    void GenerateChunkChild(Cell cell)
    {

        if (cell.chunk.down && _cells[cell.x, cell.y - 1].chunk == null) {
            Rule rule = GetRulesForNextChunk(cell.x, cell.y - 1);

            GameObject newChunk = SelectChunk(rule);

            if (newChunk != null) {
                GameObject instance = Instantiate(SelectChunk(rule));

                instance.name = "Chunk (" + cell.x + ", " + (cell.y - 1) + ")";
                instance.transform.position = new Vector2(_sizeX * (cell.x) + _sizeX / 2f, _sizeY * (cell.y - 1) + _sizeY / 2f);

                _cells[cell.x, cell.y - 1].chunk = instance.GetComponent<Chunck>();

                GenerateChunkChild(_cells[cell.x, cell.y - 1]);
            }
        }

        if (cell.chunk.up && _cells[cell.x, cell.y + 1].chunk == null) {
            Rule rule = GetRulesForNextChunk(cell.x, cell.y + 1);

            GameObject newChunk = SelectChunk(rule);

            if (newChunk != null) {
                GameObject instance = Instantiate(SelectChunk(rule));
                instance.name = "Chunk (" + cell.x + ", " + (cell.y + 1) + ")";
                instance.transform.position = new Vector2(_sizeX * (cell.x) + _sizeX / 2f, _sizeY * (cell.y + 1) + _sizeY / 2f);

                _cells[cell.x, cell.y + 1].chunk = instance.GetComponent<Chunck>();

                GenerateChunkChild(_cells[cell.x, cell.y + 1]);
            }
        }

        if (cell.chunk.left && _cells[cell.x - 1, cell.y].chunk == null) {
            Rule rule = GetRulesForNextChunk(cell.x - 1, cell.y);

            GameObject newChunk = SelectChunk(rule);

            if (newChunk != null) {
                GameObject instance = Instantiate(SelectChunk(rule));
                instance.name = "Chunk (" + (cell.x - 1) + ", " + (cell.y) + ")";
                instance.transform.position = new Vector2(_sizeX * (cell.x - 1) + _sizeX / 2f, _sizeY * (cell.y) + _sizeY / 2f);

                _cells[cell.x - 1, cell.y].chunk = instance.GetComponent<Chunck>();

                GenerateChunkChild(_cells[cell.x - 1, cell.y]);
            }
        }

        if (cell.chunk.right && _cells[cell.x + 1, cell.y].chunk == null) {
            Rule rule = GetRulesForNextChunk(cell.x + 1, cell.y);

            GameObject newChunk = SelectChunk(rule);

            if (newChunk != null) {
                GameObject instance = Instantiate(SelectChunk(rule));
                instance.name = "Chunk (" + (cell.x + 1) + ", " + (cell.y) + ")";
                instance.transform.position = new Vector2(_sizeX * (cell.x + 1) + _sizeX / 2f, _sizeY * (cell.y) + _sizeY / 2f);

                _cells[cell.x + 1, cell.y].chunk = instance.GetComponent<Chunck>();

                GenerateChunkChild(_cells[cell.x + 1, cell.y]);
            }
        }
    }

    private Rule GetRulesForNextChunk(int x, int y)
    {
        Rule rule;
        rule.up = RuleState.DONT_CARE;
        rule.left = RuleState.DONT_CARE;
        rule.right = RuleState.DONT_CARE;
        rule.down = RuleState.DONT_CARE;

        //down
        if (y > 0) {
            if (_cells[x, y - 1].chunk != null) {
                rule.down = _cells[x, y - 1].chunk.up ? RuleState.ALWAYS_TRUE : RuleState.ALWAYS_FALSE;
            }
        } else {
            rule.down = RuleState.ALWAYS_FALSE;
        }

        //left
        if (x > 0) {
            if (_cells[x - 1, y].chunk != null) {
                rule.left = _cells[x - 1, y].chunk.right ? RuleState.ALWAYS_TRUE : RuleState.ALWAYS_FALSE;
            }
        } else {
            rule.left = RuleState.ALWAYS_FALSE;
        }

        //right
        if (x < _cellNbX - 1) {
            if (_cells[x + 1, y].chunk != null) {
                rule.right = _cells[x + 1, y].chunk.left ? RuleState.ALWAYS_TRUE : RuleState.ALWAYS_FALSE;
            }
        } else {
            rule.right = RuleState.ALWAYS_FALSE;
        }

        //up
        if (y < _cellNbY - 1) {
            if (_cells[x, y + 1].chunk != null) {
                rule.up = _cells[x, y + 1].chunk.down ? RuleState.ALWAYS_TRUE : RuleState.ALWAYS_FALSE;
            }
        } else {
            rule.up = RuleState.ALWAYS_FALSE;
        }

        return rule;
    }

    private GameObject SelectChunk(Rule rule)
    {
        List<GameObject> possibleNewChunk = (from t in _chunksAvailable
            let currentChunk = t.GetComponent<Chunck>()
            where rule.down != RuleState.ALWAYS_TRUE || currentChunk.down
            where rule.down != RuleState.ALWAYS_FALSE || !currentChunk.down
            where rule.up != RuleState.ALWAYS_TRUE || currentChunk.up
            where rule.up != RuleState.ALWAYS_FALSE || !currentChunk.up
            where rule.left != RuleState.ALWAYS_TRUE || currentChunk.left
            where rule.left != RuleState.ALWAYS_FALSE || !currentChunk.left
            where rule.right != RuleState.ALWAYS_TRUE || currentChunk.right
            where rule.right != RuleState.ALWAYS_FALSE || !currentChunk.right
            select t).ToList();

        return possibleNewChunk.Count == 0 ? null : possibleNewChunk[Random.Range(0, possibleNewChunk.Count)];
    }

    private void OnDrawGizmos()
    {
        for (int x = 0; x < _cellNbX; x++) {
            for (int y = 0; y < _cellNbY; y++) {
                Gizmos.DrawWireCube(new Vector3(x * _sizeX + _sizeX / 2f, y * _sizeY + _sizeY / 2f), new Vector3(_sizeX, _sizeY));
            }
        }
    }
}