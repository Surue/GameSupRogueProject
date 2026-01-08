using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public static class IListExtensions {
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }
}

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private int _cellNbX = 20;
    [SerializeField] private int _cellNbY = 20;
    
    [SerializeField][Range(0, 10)] private float _sizeCell = 1;

    [SerializeField] private bool _useRandomNeighborsOrder = false;

    [Header("Prefabs")] 
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _cellingPrefab;

    private enum GenerationType {
        BFS,
        DFS,
        Backtrace,
        Kruskal
    }

   [SerializeField] private GenerationType _generationType;
    
    private enum CellType {
        Start,
        End,
        Passage,
        Wall
    }
    
    private struct Cell {
        public List<int> NeighborsIndices;
        public List<float> Weights;
        public CellType CellType;
    }

    private Cell[] cells_;
    
    private void Start()
    {
        BoundsInt bounds = new (-1, -1, 0, 3, 3, 1);
        
        cells_ = new Cell[_cellNbX * _cellNbY];
        
        for (int x = 0; x < _cellNbX; x++) {
            for(int y = 0; y < _cellNbY; y++) {

                int index = PosToIndex(x, y);
                
                //Default type is wall
                cells_[index].CellType = CellType.Wall;
                
                //Get neighbors indices
                cells_[index].NeighborsIndices = new List<int>(4);
                cells_[index].Weights = new List<float>(4);
                
                foreach (Vector3Int pos in bounds.allPositionsWithin) {
                    //Check if is bounds or array
                    if(x + pos.x < 0 || x + pos.x >= _cellNbX) continue;
                    if(y + pos.y < 0 || y + pos.y >= _cellNbY) continue;
                    
                    //Ignore self
                    if(pos.x == 0 && pos.y == 0) continue;
                    
                    //Taking only the cross
                    if(pos.x != 0 && pos.y != 0) continue;
                    
                    //Add neighbors
                    cells_[index].NeighborsIndices.Add(PosToIndex(x + pos.x, y + pos.y));
                    cells_[index].Weights.Add(Random.Range(0f, 1f));
                }
                
                if(_useRandomNeighborsOrder)
                    cells_[index].NeighborsIndices.Shuffle();
            }
        }

        if (_generationType == GenerationType.Backtrace) {
            StartCoroutine(BuildMazeBacktrace());
        }
        else if (_generationType == GenerationType.Kruskal)
        {
            StartCoroutine(BuildMazeKruskal());
        } 
        else {
            StartCoroutine(BuildMaze());
        }
    }
    
    private int[] parents_;
    
    private int Find(int i)
    {
        if (parents_[i] == i)
            return i;
        
        return parents_[i] = Find(parents_[i]);
    }

    private void Union(int i, int j)
    {
        int rootI = Find(i);
        int rootJ = Find(j);
        
        if (rootI != rootJ)
        {
            parents_[rootI] = rootJ;
        }
    }

    private IEnumerator BuildMazeKruskal()
    {
        parents_ = new int[cells_.Length];
        for (int i = 0; i < cells_.Length; i++)
        {
            parents_[i] = i; 
            cells_[i].CellType = CellType.Passage;
        }

        cells_[0].CellType = CellType.Start;
        
        var edges = new List<(float weight, int cellA, int cellB)>();

        for (int cellIndex = 0; cellIndex < cells_.Length; cellIndex++)
        {
            Cell cell = cells_[cellIndex];
            
            for (int neighborIdx = 0; neighborIdx < cell.NeighborsIndices.Count; neighborIdx++)
            {
                int neighborIndex = cell.NeighborsIndices[neighborIdx];
                
                if (cellIndex < neighborIndex)
                {
                    float weight = cell.Weights[neighborIdx];
                    edges.Add((weight, cellIndex, neighborIndex));
                }
            }
        }
        
        edges.Sort((a, b) => a.weight.CompareTo(b.weight));

        int passagesCreated = 0;
        int maxPassages = cells_.Length - 1; 
        
        foreach (var edge in edges)
        {
            int cellA = edge.cellA;
            int cellB = edge.cellB;

            if (Find(cellA) != Find(cellB))
            {
                Union(cellA, cellB);
                passagesCreated++;

                Cell cA = cells_[cellA];
                for (int i = 0; i < cA.NeighborsIndices.Count; i++)
                {
                    if (cA.NeighborsIndices[i] == cellB)
                    {
                        cA.Weights[i] = float.MaxValue;
                        break;
                    }
                }
                cells_[cellA] = cA;

                Cell cB = cells_[cellB];
                for (int i = 0; i < cB.NeighborsIndices.Count; i++)
                {
                    if (cB.NeighborsIndices[i] == cellA)
                    {
                        cB.Weights[i] = float.MaxValue;
                        break;
                    }
                }
                cells_[cellB] = cB;
                
                yield return new WaitForSeconds(0.01f);
            }
            
            if (passagesCreated >= maxPassages)
            {
                break;
            }
        }
        
        int endIndex = 0;
        float maxDistance = 0;
        for (int i = 0; i < cells_.Length; i++)
        {
            float distance = Vector2.Distance(IndexToWorldPos(0), IndexToWorldPos(i));
            if (cells_[i].CellType == CellType.Passage && distance > maxDistance) 
            {
                maxDistance = distance;
                endIndex = i;
            }
        }
        cells_[endIndex].CellType = CellType.End;
        cells_[0].CellType = CellType.Start;
        
        AddObjectsOnlyPassage();
    }

    private IEnumerator BuildMazeBacktrace() 
    {
        //Generate starting pos
        cells_[0].CellType = CellType.Start;
        yield return new WaitForSeconds(1);

        //Link case together
        Stack<int> stack = new Stack<int>( );
        stack.Push(0);
        cells_[0].CellType = CellType.Passage;

        float maxDistance = 0;
        int endIndex = 0;

        while (stack.Count > 0) {
            int currentCell = stack.Pop();
            
            List<int> possibleNextCells = new List<int>();
            foreach (int neighborsIndex in cells_[currentCell].NeighborsIndices) {
                if (cells_[neighborsIndex].CellType == CellType.Wall) {
                    bool canBeAddedToPassage = true;
                    
                    foreach (int neighborsIndex2 in cells_[neighborsIndex].NeighborsIndices) {
                        if (cells_[neighborsIndex2].CellType == CellType.Wall) continue;
                        
                        if(neighborsIndex2 == currentCell) continue;
                        
                        canBeAddedToPassage = false;
                        break;
                    }

                    if (canBeAddedToPassage) {
                        possibleNextCells.Add(neighborsIndex);
                    }
                }
            }

            if (possibleNextCells.Count > 0) {
                int chosenCell = possibleNextCells[Random.Range(0, possibleNextCells.Count)];
                cells_[chosenCell].CellType = CellType.Passage;
                stack.Push(currentCell);
                stack.Push(chosenCell);

                float distance = Vector2.Distance(IndexToWorldPos(0), IndexToWorldPos(chosenCell));
                if (distance > maxDistance) {
                    maxDistance = distance;
                    endIndex = chosenCell;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        //Select end pos
        cells_[endIndex].CellType = CellType.End;
        cells_[0].CellType = CellType.Start;

        AddObjects();
    }

    private IEnumerator BuildMaze() {
        //Generate starting pos
        cells_[0].CellType = CellType.Start;
        yield return new WaitForSeconds(1);

        //Link case together
        List<int> openList = new List<int> {0};
        List<int> closedList = new List<int>();

        float maxDistance = 0;
        int endIndex = 0;
        
        while (openList.Count > 0) 
        {
            int indexToSelectFrom = 0;

            if (_generationType == GenerationType.BFS) 
            {
                indexToSelectFrom = 0;
            }
            else if (_generationType == GenerationType.DFS) 
            {
                indexToSelectFrom = openList.Count - 1;
            }
            
            int index = openList[indexToSelectFrom];
            
            closedList.Add(index);
            openList.RemoveAt(indexToSelectFrom);

            int nonWalledCell = 0;
            List<int> possibleNeighbors = new List<int>();
            foreach (int neighborsIndex in cells_[index].NeighborsIndices) 
            {
                if(cells_[neighborsIndex].CellType != CellType.Wall) 
                {
                    nonWalledCell++;
                } 
                else 
                {
                    if (!openList.Contains(neighborsIndex) && !closedList.Contains(neighborsIndex)) 
                    {
                        possibleNeighbors.Add(neighborsIndex);
                    }
                }
            }

            if (nonWalledCell <= 1) {
                cells_[index].CellType = CellType.Passage;
                openList.AddRange(possibleNeighbors);
                
                float distance = Vector2.Distance(IndexToWorldPos(0), IndexToWorldPos(index));
                if (distance > maxDistance) 
                {
                    maxDistance = distance;
                    endIndex = index;
                }
            } 
            
            yield return new WaitForSeconds(0.1f);
        }

        //Select end pos
        cells_[endIndex].CellType = CellType.End;
        cells_[0].CellType = CellType.Start;

        AddObjects();
    }

    private void AddObjectsOnlyPassage()
    {
        for (int cellIndex = 0; cellIndex < cells_.Length; cellIndex++)
        {
            Cell cell = cells_[cellIndex];
            

            for (int i = 0; i < cell.NeighborsIndices.Count; i++)
            {
                int neighborIndex = cell.NeighborsIndices[i];
                Cell neighbor = cells_[neighborIndex];
                
                // --- La Ligne Clé Corrigée ---
                // Vérifier si un mur doit être placé (si l'arête n'a PAS été sélectionnée par Kruskal)
                bool isWall = cell.Weights[i] < float.MaxValue; 
                // -----------------------------

                // L'ancienne vérification était : if (neighbor.CellType == CellType.Wall)
                if (isWall) 
                {
                    Vector2 selfPos = IndexToWorldPos(cellIndex);
                    Vector2 neighborPos = IndexToWorldPos(neighborIndex);
                    
                    // (Le reste de la logique de placement des murs reste le même)
                    
                    // TOP
                    if (selfPos.y < neighborPos.y)
                    {
                        GameObject instance = Instantiate(_cellingPrefab);
                        instance.transform.position = selfPos + new Vector2(0f, _sizeCell * 0.5f);
                    }
                    
                    // BOTTOM
                    if (selfPos.y > neighborPos.y)
                    {
                        GameObject instance = Instantiate(_floorPrefab);
                        instance.transform.position = selfPos + new Vector2(0f, -_sizeCell * 0.5f);
                    }
                    
                    // LEFT
                    if (selfPos.x > neighborPos.x)
                    {
                        GameObject instance = Instantiate(_wallPrefab);
                        instance.transform.position = selfPos + new Vector2(-_sizeCell * 0.5f, 0);
                    }
                    
                    // RIGHT
                    if (selfPos.x < neighborPos.x)
                    {
                        GameObject instance = Instantiate(_wallPrefab);
                        instance.transform.position = selfPos + new Vector2(_sizeCell * 0.5f, 0);
                    }
                }
            }
            
            if (cell.NeighborsIndices.Count < 4)
            {
                bool hasTop = false;
                bool hasBottom = false;
                bool hasLeft = false;
                bool hasRight = false;

                Vector2 selfPos = IndexToWorldPos(cellIndex);
                for (int i = 0; i < cell.NeighborsIndices.Count; i++)
                {
                    int neighborIndex = cell.NeighborsIndices[i];
                    Cell neighbor = cells_[neighborIndex];
                    
                    Vector2 neighborPos = IndexToWorldPos(neighborIndex);

                    if (selfPos.y != neighborPos.y)
                    {
                        if (selfPos.y < neighborPos.y)
                        {
                            hasTop = true;
                        }else if (selfPos.y > neighborPos.y)
                        {
                            hasBottom = true;
                        }
                    }

                    if (selfPos.x != neighborPos.x)
                    {
                        if (selfPos.x > neighborPos.x)
                        {
                            hasLeft = true;
                        }else if (selfPos.x < neighborPos.x)
                        {
                            hasRight = true;
                        }
                    }
                }
                
                // TOP
                if (!hasTop)
                {
                    GameObject instance = Instantiate(_cellingPrefab);
                    instance.transform.position = selfPos + new Vector2(0f, _sizeCell * 0.5f);
                }
                    
                // BOTTOM
                if (!hasBottom)
                {
                    GameObject instance = Instantiate(_floorPrefab);
                    instance.transform.position = selfPos + new Vector2(0f, -_sizeCell * 0.5f);
                }
                    
                // LEFT
                if (!hasLeft)
                {
                    GameObject instance = Instantiate(_wallPrefab);
                    instance.transform.position = selfPos + new Vector2(-_sizeCell * 0.5f, 0);
                }
                    
                // RIGHT
                if (!hasRight)
                {
                    GameObject instance = Instantiate(_wallPrefab);
                    instance.transform.position = selfPos + new Vector2(_sizeCell * 0.5f, 0);
                }
            }
        }
    }
    
    private void AddObjects()
    {
        for (int cellIndex = 0; cellIndex < cells_.Length; cellIndex++)
        {
            Cell cell = cells_[cellIndex];
            if(cell.CellType == CellType.Wall) continue;
            for (int i = 0; i < cell.NeighborsIndices.Count; i++)
            {
                int neighborIndex = cell.NeighborsIndices[i];
                Cell neighbor = cells_[neighborIndex];
                if (neighbor.CellType == CellType.Wall)
                {
                    Vector2 selfPos = IndexToWorldPos(cellIndex);
                    Vector2 neighborPos = IndexToWorldPos(neighborIndex);
                    
                    // TOP
                    if (selfPos.y < neighborPos.y)
                    {
                        GameObject instance = Instantiate(_cellingPrefab);
                        instance.transform.position = selfPos + new Vector2(0f, _sizeCell * 0.5f);
                    }
                    
                    // BOTTOM
                    if (selfPos.y > neighborPos.y)
                    {
                        GameObject instance = Instantiate(_floorPrefab);
                        instance.transform.position = selfPos + new Vector2(0f, -_sizeCell * 0.5f);
                    }
                    
                    // LEFT
                    if (selfPos.x > neighborPos.x)
                    {
                        GameObject instance = Instantiate(_wallPrefab);
                        instance.transform.position = selfPos + new Vector2(-_sizeCell * 0.5f, 0);
                    }
                    
                    // RIGHT
                    if (selfPos.x < neighborPos.x)
                    {
                        GameObject instance = Instantiate(_wallPrefab);
                        instance.transform.position = selfPos + new Vector2(_sizeCell * 0.5f, 0);
                    }
                }
            }
            
            if (cell.NeighborsIndices.Count < 4)
            {
                bool hasTop = false;
                bool hasBottom = false;
                bool hasLeft = false;
                bool hasRight = false;

                Vector2 selfPos = IndexToWorldPos(cellIndex);
                for (int i = 0; i < cell.NeighborsIndices.Count; i++)
                {
                    int neighborIndex = cell.NeighborsIndices[i];
                    Cell neighbor = cells_[neighborIndex];
                    
                    Vector2 neighborPos = IndexToWorldPos(neighborIndex);

                    if (selfPos.y != neighborPos.y)
                    {
                        if (selfPos.y < neighborPos.y)
                        {
                            hasTop = true;
                        }else if (selfPos.y > neighborPos.y)
                        {
                            hasBottom = true;
                        }
                    }

                    if (selfPos.x != neighborPos.x)
                    {
                        if (selfPos.x > neighborPos.x)
                        {
                            hasLeft = true;
                        }else if (selfPos.x < neighborPos.x)
                        {
                            hasRight = true;
                        }
                    }
                }
                
                // TOP
                if (!hasTop)
                {
                    GameObject instance = Instantiate(_cellingPrefab);
                    instance.transform.position = selfPos + new Vector2(0f, _sizeCell * 0.5f);
                }
                    
                // BOTTOM
                if (!hasBottom)
                {
                    GameObject instance = Instantiate(_floorPrefab);
                    instance.transform.position = selfPos + new Vector2(0f, -_sizeCell * 0.5f);
                }
                    
                // LEFT
                if (!hasLeft)
                {
                    GameObject instance = Instantiate(_wallPrefab);
                    instance.transform.position = selfPos + new Vector2(-_sizeCell * 0.5f, 0);
                }
                    
                // RIGHT
                if (!hasRight)
                {
                    GameObject instance = Instantiate(_wallPrefab);
                    instance.transform.position = selfPos + new Vector2(_sizeCell * 0.5f, 0);
                }
            }
        }
    }

    private int PosToIndex(int x, int y) {
        return x * _cellNbX + y;
    }

    private Vector2 IndexToPos(int index) 
    {
        int x = index / _cellNbX;
        int y = index % _cellNbX;
        
        return new Vector2(x, y);
    }
    
    private Vector2 IndexToWorldPos(int index) 
    {
        int x = index / _cellNbX;
        int y = index % _cellNbX;
        
        return new Vector2(x * _sizeCell + (_sizeCell * 0.5f), y * _sizeCell + (_sizeCell * 0.5f));
    }

    private void OnDrawGizmos() 
    {
        if (cells_ == null) return;
        
        for (int i = 0; i < _cellNbX * _cellNbY; i++) 
        {
            switch (cells_[i].CellType) {
                case CellType.Start:
                    Gizmos.color = Color.blue;
                    break;
                case CellType.End:
                    Gizmos.color = Color.red;
                    break;
                case CellType.Passage:
                    Gizmos.color = Color.white;
                    break;
                case CellType.Wall:
                    Gizmos.color = Color.black;
                    break;
                default:
                    Gizmos.color = Color.black;
                    break;
            }
            Gizmos.DrawCube(IndexToWorldPos(i), new Vector3(_sizeCell, _sizeCell));
        }
    }
}
