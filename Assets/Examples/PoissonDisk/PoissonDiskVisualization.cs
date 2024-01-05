using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PoissonDiskVisualization : MonoBehaviour {
    [SerializeField] private Vector2 _sampleRegionSize;
    [SerializeField] private float _radius = 1;
    [SerializeField] private int _rejectionNumber = 30;

    [SerializeField] private bool _displayGrid = true;

    private int[,] _grid;
    private List<Vector2> _points;
    private float _cellSize;

    private void OnValidate() {
        _cellSize = _radius / Mathf.Sqrt(2);
        //Build the grid 
        _grid = new int[Mathf.CeilToInt(_sampleRegionSize.x / _cellSize), Mathf.CeilToInt(_sampleRegionSize.y / _cellSize)];

        _points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2> {
            _sampleRegionSize / 2 // Center point
        };

        while (spawnPoints.Count > 0) {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
    
            Vector2 spawnCenter = spawnPoints[spawnIndex];

            bool candidateValid = false;

            for (int i = 0; i < _rejectionNumber; i++) {
                float angle = Random.value * Mathf.PI * 2;
                
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCenter + dir * Random.Range(_radius, 2 * _radius);

                if (!IsValid(candidate)) continue;
                //If the candidate is valid (it's not in a radius of another object)
                _points.Add(candidate);
                spawnPoints.Add(candidate);
                _grid[(int)(candidate.x / _cellSize), (int)(candidate.y / _cellSize)] = _points.Count; //Index of the added point
                candidateValid = true;
                break;
            }
            
            //If the candidate is invalid, then remove the spawn point
            if (!candidateValid) {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
    }

    private bool IsValid(Vector2 candidate) {
        if (candidate.x < 0 || candidate.x >= _sampleRegionSize.x || candidate.y < 0 || candidate.y >= _sampleRegionSize.y) return false;
        
        //Cell currentPosition
        int cellX = (int)(candidate.x / _cellSize);
        int cellY = (int)(candidate.y / _cellSize);
    
        //Goes from a 5 by 5 square around the position
        int searchStartX = Mathf.Max(0, cellX - 2);
        int searchEndX = Mathf.Min(cellX + 2, _grid.GetLength(0) - 1);
        
        int searchStartY = Mathf.Max(0, cellY - 2);
        int searchEndY = Mathf.Min(cellY + 2, _grid.GetLength(1) - 1);
        
        //Loop through each square adjacent
        for (int x = searchStartX; x <= searchEndX; x++) {
            for (int y = searchStartY; y <= searchEndY; y++) {
                
                int pointIndex = _grid[x, y] - 1;
                
                //If the point index has no assignation yet (by default the value == 0 then minus 1 goes to == -1)
                if (pointIndex == -1) continue;
                
                //Check if the square distance between the point in the grid and the candidate is valid
                float dist = (candidate - _points[pointIndex]).sqrMagnitude;
                    
                if (dist < _radius * _radius) {
                    return false;
                }
            }
        }
        
        return true;
    }

    private void OnDrawGizmos() {
        if (_points == null || _points.Count <= 0) return;
        
        Gizmos.DrawWireCube(_sampleRegionSize/2.0f, _sampleRegionSize);
            
        foreach (Vector2 vector2 in _points) {
            Gizmos.DrawWireSphere(vector2, _radius * 0.5f);
        }

        if (!_displayGrid) return;

        Gizmos.color = Color.red;
        for (int x = 0; x < _grid.GetLength(0); x++) {
            for (int y = 0; y < _grid.GetLength(1); y++) {
                if (_grid[x, y] == 0) {
                    Gizmos.DrawWireCube(
                        new Vector3(x * _cellSize + _cellSize * 0.5f, y * _cellSize + _cellSize * 0.5f, 0),
                        new Vector3(_cellSize, _cellSize, _cellSize));
                    Gizmos.DrawLine(
                        new Vector3(x * _cellSize, y * _cellSize, 0),
                        new Vector3(x * _cellSize + _cellSize, y * _cellSize + _cellSize, 0));
                    
                    Gizmos.DrawLine(
                        new Vector3(x * _cellSize + _cellSize, y * _cellSize, 0),
                        new Vector3(x * _cellSize, y * _cellSize + _cellSize, 0));
                }
            }
        }
        
        Gizmos.color = Color.white;
        for (int x = 0; x < _grid.GetLength(0); x++) {
            for (int y = 0; y < _grid.GetLength(1); y++) {
                if(_grid[x,y] != 0)
                    Gizmos.DrawWireCube(new Vector3(x * _cellSize + _cellSize * 0.5f, y * _cellSize + _cellSize * 0.5f, 0), new Vector3(_cellSize, _cellSize, _cellSize));
            }
        }
    }
}
