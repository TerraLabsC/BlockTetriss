using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

public class Board : MonoBehaviour
{
  public const int Size = 15;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Transform cellTransform;
    private readonly Cell[,] cells = new Cell[Size, Size];
    private readonly int[,] data = new int[Size, Size];
    private readonly List<Vector2Int> hoverPoints = new();

    private readonly List<int> fullLineColums = new();
    private readonly List<int> fullLineRows = new();

    private void Start()
    {
        for(var r = 0; r < Size; ++r)
        {
            for (var c = 0; c < Size; ++c)
            {
                cells[r, c] = Instantiate(cellPrefab, cellTransform);
                cells[r, c].transform.position = new(c + 0.5f, r + 0.5f, 0.0f);
                cells[r, c].Hide();
            }
        }
    }

    public void Hover(Vector2Int point, int polyominoIndex, int colorIndex)
    {
        var polyomino = Polyominus.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var poluominoColumns = polyomino.GetLength(1);
        Unhover();
        HoverPoints(point, polyominoRows, poluominoColumns, polyomino);
        if(hoverPoints.Count > 0)
        {
            Hover(colorIndex);
        }
    }

    private void HoverPoints(Vector2Int point, int polyonimoRows, int polyonimoColums, int[,] polyonimo)
    {
        for (var r = 0; r < polyonimoRows; ++r)
        {
            for (var c = 0; c < polyonimoColums; ++c)
            {
                if (polyonimo[r, c] > 0)
                {
                    var hoverPont = point + new Vector2Int(c, r); 

                    if(isValidPoint(hoverPont) == false)
                    {
                        hoverPoints.Clear();
                        return;
                    }

                    hoverPoints.Add(hoverPont);
                }
            }
        }
    }

    private bool isValidPoint(Vector2Int point)
    {
        if (point.x < 0 || Size <= point.x) return false;
        if (point.y < 0 || Size <= point.y) return false;
        if (data[point.y, point.x] > 0) return false;

        return true;
    }

    private void Hover(int colorIndex)
    {
        foreach (var hoverPoint in hoverPoints)
        {
            data[hoverPoint.y, hoverPoint.x] = 1;

            cells[hoverPoint.y, hoverPoint.x].SetColor(colorIndex);
            cells[hoverPoint.y, hoverPoint.x].Hover();
        }
    }

    private void Unhover()
    {
        foreach (var hoverPoint in hoverPoints)
        {
            data[hoverPoint.y, hoverPoint.x] = 0;
            cells[hoverPoint.y, hoverPoint.x].Hide();
        }
        hoverPoints.Clear();
    }

    public bool Place(Vector2Int point, int polyominoIndex, int colorIndex)
    {
        var polyomino = Polyominus.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var poluominoColumns = polyomino.GetLength(1);
        Unhover();
        HoverPoints(point, polyominoRows, poluominoColumns, polyomino);
        if (hoverPoints.Count > 0)
        {
            Place(point, poluominoColumns, polyominoRows, colorIndex);
            return true;
        }
        return false;
    }

    private void Place(Vector2Int point, int poluominoColumns, int polyominoRows, int colorIndex)
    {
        foreach (var hoverPoint in hoverPoints)
        {
            data[hoverPoint.y, hoverPoint.x] = 1;

            cells[hoverPoint.y, hoverPoint.x].SetColor(colorIndex);
            cells[hoverPoint.y, hoverPoint.x].Normal();
        }
        ClearFullLines(point, poluominoColumns, polyominoRows);
        hoverPoints.Clear();
    }

    private void ClearFullLines(Vector2Int point, int poluominoColumns, int polyominoRows)
    {
        var fromColums = Mathf.Max(0, point.x);
        var toColumsExclusive = Mathf.Min(Size, point.x + poluominoColumns);
        
        var fromRow = Mathf.Max(0, point.y);
        var toRowExclusive = Mathf.Min(Size, point.y + polyominoRows);

        FullLineColums(fromColums, toColumsExclusive);
        FullLineRows(fromRow, toRowExclusive);

        const int squareSize = 5;
        int offset = Size - squareSize;
        
        CheckSquare(0, 0, squareSize); // Левый нижний
        
        CheckSquare(0, offset, squareSize); // Левый верхний
        
        CheckSquare(offset, 0, squareSize); // Правый нижний
        
        CheckSquare(offset, offset, squareSize); // Правый верхний

        CheckSquare(offset / 2, offset / 2, squareSize); // Центральный

        ClearFullLinesColums();
        ClearFullLinesRows();
    }

    private void FullLineColums(int fromColums, int toColumsExclusive)
    {
        fullLineColums.Clear();

        for (var c = fromColums; c < toColumsExclusive; ++c)
        {
            var isFullLine = true;

            for (var r = 0; r < Size; ++r)
            {
                if (data[r, c] == 0)
                {
                    isFullLine = false;
                    break; 
                }
            }

            if (isFullLine == true)
            {
                fullLineColums.Add(c);
            }
        }
    }

    private void FullLineRows(int fromRow, int toRowExclusive)
    {
        fullLineRows.Clear();

        for (var r = fromRow; r < toRowExclusive; ++r)
        {
            var isFullLine = true;

            for (var c = 0; c < Size; ++c)
            {
                if (data[r, c] == 0)
                {
                    isFullLine = false;
                    break;
                }
            }

            if (isFullLine == true)
            {
                fullLineRows.Add(r);
            }
        }
    }

    private void ClearFullLinesColums()
    {
        foreach(var c in fullLineColums)
        {
            for (var r = 0; r < Size; ++r)
            {
                data[r, c] = 0;
                cells[r, c].Hide();
            }
        }
    }

    private void ClearFullLinesRows()
    {
        foreach (var r in fullLineRows)
        {
            for (var c = 0; c < Size; ++c)
            {
                data[r, c] = 0;
                cells[r, c].Hide();
            }
        }
    }

    private void CheckSquare(int startX, int startY, int size)
    {
        bool isFull = true;

        for (var r = startY; r < startY + size; ++r)
        {
            for (var c = startX; c < startX + size; ++c)
            {
                if (data[r, c] == 0)
                {
                    isFull = false;
                    break;
                }
            }
            if (!isFull) break;
        }

        if (isFull)
        {
            for (var r = startY; r < startY + size; ++r)
            {
                for (var c = startX; c < startX + size; ++c)
                {
                    data[r, c] = 0;
                    cells[r, c].Hide();
                }
            }
        }
    }
}
