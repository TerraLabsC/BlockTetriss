using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [SerializeField] private TextMeshProUGUI TextCount;

    private int score = 0;
    private readonly int minSquareScore = 23000;
    private readonly int maxSquareScore = 30000;
    private readonly int minLineScore = 5000;
    private readonly int maxLineScore = 10000;

    [SerializeField] private TextMeshProUGUI textMeshProUGUIWinAndLose;
    [SerializeField] private GameObject win;
    [SerializeField] private GameObject lose;

    [SerializeField] private GameObject CanvasWinAndLose;
    [SerializeField] private TextMeshProUGUI CountText;

    [SerializeField] private GameObject zone;
    
    [SerializeField] private Blocks blocks;

    private bool isGameOver = false;
    private bool isWin = false;
    private bool isLose = false;

    [SerializeField] private int winScore = 10000000;

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
        
        UpdateScoreText();
    }

    private void Update()
    {
        CheckGameState();
    }

    /// <summary>
    /// Проверяет состояние игры (победа/поражение)
    /// </summary>
    private void CheckGameState()
    {
        if (isGameOver) return;

        if (score >= winScore && !isWin)
        {
            Win();
        }

        if (!isLose && blocks != null && blocks.HasNoAvailableMoves(this))
        {
            Lose();
        }
    }

    /// <summary>
    /// Проверяет, может ли блок быть размещён в данной позиции на поле
    /// </summary>
    public bool CanPlace(Vector2Int point, int polyominoIndex)
    {
        if (isGameOver) return false;
        
        var polyomino = Polyominus.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var poluominoColumns = polyomino.GetLength(1);

        for (var r = 0; r < polyominoRows; ++r)
        {
            for (var c = 0; c < poluominoColumns; ++c)
            {
                if (polyomino[r, c] > 0)
                {
                    var checkPoint = point + new Vector2Int(c, r);

                    if (checkPoint.x < 0 || Size <= checkPoint.x || 
                        checkPoint.y < 0 || Size <= checkPoint.y)
                    {
                        return false;
                    }

                    if (data[checkPoint.y, checkPoint.x] > 0)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void Hover(Vector2Int point, int polyominoIndex, int colorIndex)
    {
        if (isGameOver) return;
        
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
        if (isGameOver) return false;
        
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

        CheckGameState();
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

        int squaresCleared = 0;
        
        if (CheckSquare(0, 0, squareSize)) squaresCleared++; // Левый нижний
        if (CheckSquare(0, offset, squareSize)) squaresCleared++; // Левый верхний
        if (CheckSquare(offset, 0, squareSize)) squaresCleared++; // Правый нижний
        if (CheckSquare(offset, offset, squareSize)) squaresCleared++; // Правый верхний
        if (CheckSquare(offset / 2, offset / 2, squareSize)) squaresCleared++; // Центральный

        if (CheckSquare(offset / 2, offset, squareSize)) squaresCleared++; // Центральный верхний
        if (CheckSquare(offset, offset / 2, squareSize)) squaresCleared++; // Центральный правый
        if (CheckSquare(0, offset / 2, squareSize)) squaresCleared++; // Центральный левый
        if (CheckSquare(offset / 2, 0, squareSize)) squaresCleared++; // Центральный нижний

        if (squaresCleared > 0)
        {
            int squarePoints = Random.Range(minSquareScore, maxSquareScore + 1);
            AddScore(squarePoints * squaresCleared);
            Debug.Log($"Квадраты: +{squarePoints * squaresCleared} очков ({squaresCleared} квадратов)");
        }

        int linesCleared = fullLineColums.Count + fullLineRows.Count;
        if (linesCleared > 0)
        {
            int linePoints = Random.Range(minLineScore, maxLineScore + 1);
            AddScore(linePoints * linesCleared);
            Debug.Log($"Линии: +{linePoints * linesCleared} очков ({linesCleared} линий)");
        }

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

    private bool CheckSquare(int startX, int startY, int size)
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
            return true;
        }
        
        return false;
    }

    private void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
        
        if (score >= winScore && !isGameOver)
        {
            Win();
        }
    }

    private void UpdateScoreText()
    {
        if (TextCount != null)
        {
            TextCount.text = score.ToString("N0");
        }
    }

    public int GetScore() => score;

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }
    
    /// <summary>
    /// Очищает всё поле (для новой игры)
    /// </summary>
    public void ClearBoard()
    {
        for (var r = 0; r < Size; ++r)
        {
            for (var c = 0; c < Size; ++c)
            {
                data[r, c] = 0;
                cells[r, c].Hide();
            }
        }
    }
    
    /// <summary>
    /// Обработка победы
    /// </summary>
    private void Win()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        isWin = true;
        
        Debug.Log($"ПОБЕДА! Достигнуто {winScore} очков! Текущий счет: {score}");
        
        if (CanvasWinAndLose != null)
        {
            CanvasWinAndLose.SetActive(true);
            
            if (textMeshProUGUIWinAndLose != null)
            {
                win.SetActive(true);
                lose.SetActive(false);
            }
            
            if (CountText != null)
            {
                CountText.text = $"Ваш счет: {score}";
                zone.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Обработка поражения
    /// </summary>
    private void Lose()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        isLose = true;
        
        Debug.Log($"ПОРАЖЕНИЕ! Нет доступных ходов! Текущий счет: {score}");
        
        if (CanvasWinAndLose != null)
        {
            CanvasWinAndLose.SetActive(true);
            
            if (textMeshProUGUIWinAndLose != null)
            {
                win.SetActive(false);
                lose.SetActive(true);
            }
            
            if (CountText != null)
            {
                CountText.text = $"Ваш счет: {score}";
                zone.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Сброс игры (для рестарта)
    /// </summary>
    public void ResetGame()
    {
        ClearBoard();
        ResetScore();
        isGameOver = false;
        isWin = false;
        isLose = false;
        
        if (CanvasWinAndLose != null)
        {
            CanvasWinAndLose.SetActive(false);
        }
        
        if (blocks != null)
        {
            blocks.ResetBlocks();
        }
    }
}