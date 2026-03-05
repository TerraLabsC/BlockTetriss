using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private PositionPolyominus positionPolyominus;

    private int blockCount = 0;
    private bool isInitialized = false;

    private List<int> availablePolyominoIndices = new List<int>();

    private void Start()
    {
        if (positionPolyominus == null)
        {
            positionPolyominus = FindObjectOfType<PositionPolyominus>();
        }
        
        SetupBlocks();
        GenerateAndPosition();
    }

    private void SetupBlocks()
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Initialize();
        }
        isInitialized = true;
    }

    /// <summary>
    /// Инициализирует список доступных фигур (без повторений)
    /// </summary>
    private void InitializeAvailablePolyominos()
    {
        availablePolyominoIndices.Clear();
        int totalPolyominos = Polyominus.Length;
        int blocksCount = blocks.Length;

        for (int i = 0; i < totalPolyominos; i++)
        {
            availablePolyominoIndices.Add(i);
        }

        ShuffleList(availablePolyominoIndices);

        Debug.Log($"Инициализировано {blocksCount} уникальных фигур из {totalPolyominos} доступных");
    }

    /// <summary>
    /// Перемешивает список (алгоритм Fisher-Yates)
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    /// <summary>
    /// Получает следующий уникальный индекс фигуры
    /// </summary>
    private int GetNextUniquePolyominoIndex()
    {
        if (availablePolyominoIndices.Count == 0)
        {
            InitializeAvailablePolyominos();
        }

        int index = availablePolyominoIndices[availablePolyominoIndices.Count - 1];
        availablePolyominoIndices.RemoveAt(availablePolyominoIndices.Count - 1);

        return index;
    }

    private void GenerateAndPosition()
    {
        if (!isInitialized) return;

        InitializeAvailablePolyominos();

        for (int i = 0; i < blocks.Length; i++)
        {
            int randomIndex = GetNextUniquePolyominoIndex();

            // Все блоки в позиции 0,0
            blocks[i].transform.localPosition = Vector3.zero;
            blocks[i].transform.localScale = Vector3.one;
            blocks[i].Show(randomIndex);
            blocks[i].gameObject.SetActive(true);

            blockCount++;

            Debug.Log($"Блок {i} получил фигуру {randomIndex}");
        }

        RefreshFigurePositions();
    }

    public void Remove()
    {
        blockCount--;
        if (blockCount <= 0)
        {
            blockCount = 0;
            GenerateAndPosition();
        }
        else
        {
            RefreshFigurePositions();
        }
    }

    /// <summary>
    /// Обновляет позиции всех фигур в сетке
    /// </summary>
    private void RefreshFigurePositions()
    {
        if (positionPolyominus != null)
        {
            Debug.Log("Обновление позиций фигур после изменения состава");
            
            positionPolyominus.ForceAnalyze();
        }
        else
        {
            Debug.LogWarning("PositionPolyominus не назначен! Фигуры не будут переставлены.");
        }
    }

    public void ForceRefreshPositions()
    {
        RefreshFigurePositions();
    }

    /// <summary>
    /// Проверяет, есть ли доступные ходы для текущих фигур на доске
    /// </summary>
    /// <param name="board">Ссылка на доску</param>
    /// <returns>true - если нет доступных ходов (проигрыш), false - если ходы есть</returns>
    public bool HasNoAvailableMoves(Board board)
    {
        if (board == null)
        {
            Debug.LogError("Board не найден!");
            return false;
        }

        List<int> activePolyominoIndices = new List<int>();
        
        foreach (var block in blocks)
        {
            if (block != null && block.gameObject.activeSelf)
            {
                int index = block.GetPolyominoIndex();
                if (!activePolyominoIndices.Contains(index))
                {
                    activePolyominoIndices.Add(index);
                }
            }
        }

        if (activePolyominoIndices.Count == 0)
        {
            return false;
        }

        for (int x = -Board.Size; x < Board.Size * 2; x++)
        {
            for (int y = -Board.Size; y < Board.Size * 2; y++)
            {
                Vector2Int point = new Vector2Int(x, y);
                
                foreach (int polyominoIndex in activePolyominoIndices)
                {
                    if (board.CanPlace(point, polyominoIndex))
                    {
                        return false;
                    }
                }
            }
        }

        Debug.Log("НЕТ ДОСТУПНЫХ ХОДОВ! ИГРОК ПРОИГРАЛ!");
        return true;
    }

    /// <summary>
    /// Сбрасывает все блоки и генерирует новые (для рестарта)
    /// </summary>
    public void ResetBlocks()
    {
        foreach (var block in blocks)
        {
            if (block != null)
            {
                block.gameObject.SetActive(false);
            }
        }

        blockCount = 0;
        availablePolyominoIndices.Clear();

        GenerateAndPosition();
    }

    /// <summary>
    /// Получает количество активных блоков
    /// </summary>
    public int GetActiveBlocksCount()
    {
        int count = 0;
        foreach (var block in blocks)
        {
            if (block != null && block.gameObject.activeSelf)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Получает массив всех блоков
    /// </summary>
    public Block[] GetAllBlocks()
    {
        return blocks;
    }
}