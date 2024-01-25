using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random; // Use it instead of System.Random

/*
 * 1. Create a grid of size _gridWidth * _gridHeight
 * 2. Fill the grid with Cell
 * 3. Select a random starting cell and sets a specific color to it
 * 4. Each update colors neighboring cells
 */
public class Droplets : MonoBehaviour
{
    [Serializable]
    private class Cell // Class that represent object in the grid
    {
        public Color color;
        public Vector3Int position;
    }

    [Header("Grid")]
    [SerializeField] private int _gridWidth = 50;
    [SerializeField] private int _gridHeight = 50;

    [Header("Timer")] 
    [SerializeField] private int _minSpawnTime;
    [SerializeField] private int _maxSpawnTime;
    
    private List<Cell> _cells;

    // Coloration
    private Cell _startingCell;
    private List<Cell> _coloredCell;
    private List<Cell> _tmpNewColoredCell = new List<Cell>();
    
    // Timer
    private int _numberOfFrameBeforeNextDroplet = 0;
    
    private void Start()
    {
        // Fill the grid with CEll
        _cells = new List<Cell>(); // Create a List, still empty
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                Cell cell = new Cell(); // Create an object
                cell.color = Color.clear; // Set color to red
                cell.position = new Vector3Int(x, y, 0); // Set the position using the grid
                
                _cells.Add(cell); // Add the new cell to the List
            }
        }
        // At this point the array if full
        _coloredCell = new List<Cell>();
    }

    private void Update()
    {
        // Clear list 
        _tmpNewColoredCell.Clear();
        
        // Add new Color
        _numberOfFrameBeforeNextDroplet--;
        if (_numberOfFrameBeforeNextDroplet < 0)
        {
            _numberOfFrameBeforeNextDroplet = Random.Range(_minSpawnTime, _maxSpawnTime);
            
            _startingCell = _cells[Random.Range(0, _cells.Count)]; // Select a random starting cell
            _startingCell.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1); // Set the color randomly
            _coloredCell.Add(_startingCell); // Add it to the colored list of cell
        }
        
        // Make colors move to neighbor cells
        foreach (Cell cell in _coloredCell)
        {
            // top cell
            Vector3Int topCoord = new Vector3Int(cell.position.x, cell.position.y + 1); // Create the top coord
            if (topCoord.y < _gridHeight) // Make sure the coord doesn't go over the top of the grid
            {
                TryColorCell(topCoord, cell);
            }

            // left cell
            Vector3Int leftCoord = new Vector3Int(cell.position.x - 1, cell.position.y); // Create the left coord
            if (leftCoord.x >= 0) // Make sure the coord doesn't go to much to the left
            {
                TryColorCell(leftCoord, cell);
            }

            // bottom cell
            Vector3Int bottomCoord = new Vector3Int(cell.position.x, cell.position.y - 1); // Create the bottom coord
            if (bottomCoord.y >= 0) // The bottom coord must always stay bigger that 0
            {
                TryColorCell(bottomCoord, cell);
            }

            // right cell
            Vector3Int rightCoord = new Vector3Int(cell.position.x + 1, cell.position.y); // Create the right coord
            if (rightCoord.x < _gridWidth) // The right coord must stay smaller than the total width of the grid 
            {
                TryColorCell(rightCoord, cell);
            }
        }
        
        // Make colors fade out
        foreach (var cell in _cells)
        {
            // If the cell has just been colored => Doesn't change the coord
            if (_coloredCell.Contains(cell))
            {
                continue;
            }
            
            Color cellColor = cell.color;
            cell.color = new Color(cellColor.r, cellColor.g, cellColor.b, cellColor.a * 0.95f);
        }
        
        // Add new colored's cell to the list of colored cell
        _coloredCell.Clear();
        _coloredCell.AddRange(_tmpNewColoredCell);
    }

    /// <summary>
    /// Function that tries to set the color of the cell. It only change the color if the color is different or if
    /// the alpha value is greater on the Cell
    /// </summary>
    /// <param name="cellCoord"></param>
    /// <param name="originCell"></param>
    private void TryColorCell(Vector3Int cellCoord, Cell originCell)
    {
        Cell neighborCell = _cells[CoordToIndex(cellCoord)];

        if ((Math.Abs(neighborCell.color.r - originCell.color.r) > 0.02f ||
             Math.Abs(neighborCell.color.g - originCell.color.g) > 0.02f ||
             Math.Abs(neighborCell.color.b - originCell.color.b) > 0.02f) &&
             originCell.color.a > neighborCell.color.a)
        {
            neighborCell.color = originCell.color;
            _tmpNewColoredCell.Add(neighborCell);
        }
    }

    /// <summary>
    /// Function to transform a 2D coordinate into an index (we do this because the list is in 1D and the grid is in 2D)
    /// </summary>
    /// <param name="cellCoord"></param>
    /// <returns></returns>
    private int CoordToIndex(Vector3Int cellCoord)
    {
        return cellCoord.y + cellCoord.x * _gridWidth;
    }

    /// <summary>
    /// Function to draw gizmos in the scenes's view
    /// </summary>
    private void OnDrawGizmos()
    {
        // Early exit if the list of cell is null or empty. If we don't do this this might crash when launching the game
        if (_cells == null || _cells.Count == 0)
        {
            return;
        }

        // Loop through the grid and draw a cube for each cells
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                Cell cell = _cells[CoordToIndex(new Vector3Int(x, y, 0))]; // Get the Cell (x + y * _gridWidth transform a coordinate into an index) 
                
                // Draw a wire cube to help visualize cell
                Gizmos.color = Color.gray; 
                Gizmos.DrawWireCube(cell.position, new Vector3(1, 1, 0));
                
                // Draw any colored cell
                Gizmos.color = cell.color; // Set the color of the gizmo using the color of the cell
                Gizmos.DrawCube(cell.position, new Vector3(1, 1, 0));
            }
        }
    }
}
