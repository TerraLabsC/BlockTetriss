using CartoonFX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.ParticleSystem;

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
    private bool isClearing = false;

    [SerializeField] private int winScore = 10000000;

    private void Start()
    {
        for (var r = 0; r < Size; ++r)
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

    private void CheckGameState()
    {
        if (isGameOver) return;
        if (isClearing) return;

        if (score >= winScore && !isWin)
        {
            Win();
        }

        if (!isLose && blocks != null && blocks.HasNoAvailableMoves(this))
        {
            Lose();
        }
    }

    public bool CanPlace(Vector2Int point, int polyominoIndex)
    {
        if (isGameOver || isClearing) return false;

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
        if (isGameOver || isClearing) return;

        var polyomino = Polyominus.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var poluominoColumns = polyomino.GetLength(1);

        Unhover();
        HoverPoints(point, polyominoRows, poluominoColumns, polyomino);

        if (hoverPoints.Count > 0)
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
                    if (isValidPoint(hoverPont) == false)
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
        if (isGameOver || isClearing) return false;

        var polyomino = Polyominus.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var poluominoColumns = polyomino.GetLength(1);

        Unhover();
        HoverPoints(point, polyominoRows, poluominoColumns, polyomino);

        if (hoverPoints.Count > 0)
        {
            foreach (var hoverPoint in hoverPoints)
            {
                data[hoverPoint.y, hoverPoint.x] = 1;
                cells[hoverPoint.y, hoverPoint.x].SetColor(colorIndex);
                cells[hoverPoint.y, hoverPoint.x].Normal();
            }
            hoverPoints.Clear();

            StartCoroutine(ClearSequentially(point, poluominoColumns, polyominoRows));
            return true;
        }

        return false;
    }

    private IEnumerator ClearSequentially(Vector2Int point, int poluominoColumns, int polyominoRows)
    {
        isClearing = true;

        var fromColums = Mathf.Max(0, point.x);
        var toColumsExclusive = Mathf.Min(Size, point.x + poluominoColumns);
        var fromRow = Mathf.Max(0, point.y);
        var toRowExclusive = Mathf.Min(Size, point.y + polyominoRows);

        FullLineColums(fromColums, toColumsExclusive);
        FullLineRows(fromRow, toRowExclusive);

        const int squareSize = 5;
        int offset = Size - squareSize;

        HashSet<Vector2Int> cellsToClear = new HashSet<Vector2Int>();

        foreach (var c in fullLineColums)
        {
            for (int r = 0; r < Size; r++)
                cellsToClear.Add(new Vector2Int(c, r));
        }

        foreach (var r in fullLineRows)
        {
            for (int c = 0; c < Size; c++)
                cellsToClear.Add(new Vector2Int(c, r));
        }

        int squaresCleared = 0;
        Vector2Int[] squarePositions = new Vector2Int[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, offset),
            new Vector2Int(offset, 0),
            new Vector2Int(offset, offset),
            new Vector2Int(offset / 2, offset / 2),
            new Vector2Int(offset / 2, offset),
            new Vector2Int(offset, offset / 2),
            new Vector2Int(0, offset / 2),
            new Vector2Int(offset / 2, 0)
        };

        foreach (var pos in squarePositions)
        {
            if (IsSquareFull(pos.x, pos.y, squareSize))
            {
                squaresCleared++;
                for (int r = pos.y; r < pos.y + squareSize; r++)
                {
                    for (int c = pos.x; c < pos.x + squareSize; c++)
                    {
                        cellsToClear.Add(new Vector2Int(c, r));
                    }
                }
            }
        }

        if (squaresCleared > 0)
        {
            int squarePoints = Random.Range(minSquareScore, maxSquareScore + 1);
            AddScoreWithoutGameCheck(squarePoints * squaresCleared);
            Debug.Log($"Квадраты: +{squarePoints * squaresCleared} очков ({squaresCleared} квадратов)");
        }

        int linesCleared = fullLineColums.Count + fullLineRows.Count;
        if (linesCleared > 0)
        {
            int linePoints = Random.Range(minLineScore, maxLineScore + 1);
            AddScoreWithoutGameCheck(linePoints * linesCleared);
            Debug.Log($"Линии: +{linePoints * linesCleared} очков ({linesCleared} линий)");
        }

        foreach (var cellPos in cellsToClear)
        {
            int x = cellPos.x;
            int y = cellPos.y;
            if (data[y, x] != 0)
            {
                if (cells[y, x].particle != null)
                {
                    var perticles = Instantiate(cells[y, x].particle, cells[y, x].transform.position, Quaternion.identity);
                    perticles.Play();
                    perticles.gameObject.GetComponent<CFXR_Effect>().cameraShake.enabled = true;

                    var perticlesTraile = Instantiate(cells[y, x].trailMagnet, cells[y, x].transform.position, Quaternion.identity);
                    cells[y, x].PariclesColor();
                    perticlesTraile.Play();
                }

                data[y, x] = 0;
                cells[y, x].Hide();
                yield return new WaitForSeconds(0.05f);
            }
        }

        fullLineColums.Clear();
        fullLineRows.Clear();

        isClearing = false;

        CheckGameState();
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

    private bool IsSquareFull(int startX, int startY, int size)
    {
        for (var r = startY; r < startY + size; ++r)
        {
            for (var c = startX; c < startX + size; ++c)
            {
                if (data[r, c] == 0)
                    return false;
            }
        }
        return true;
    }

    private void AddScoreWithoutGameCheck(int points)
    {
        score += points;
        UpdateScoreText();
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

    public void ResetGame()
    {
        StopAllCoroutines();
        isClearing = false;

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