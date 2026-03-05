using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ScreenOrientation
{
    Horizontal,
    Vertical
}

public class PositionPolyominus : MonoBehaviour
{
    [SerializeField] private ScreenOrientation currentOrientation = ScreenOrientation.Horizontal;
    [SerializeField] private Collider2D HorizontalCollider;
    [SerializeField] private Collider2D VerticalCollider;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float spacing = 0.1f;
    [SerializeField] private Color gridColor = Color.gray;
    [SerializeField] private Color figureGridColor = Color.cyan;
    [SerializeField] private bool showGrid = true;
    [SerializeField] private bool showFigureGrids = true;
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Block[] blocks;

    private class FigureData
    {
        public Transform figureTransform;
        public Collider2D figureCollider;
        public Color figureColor;
        public string figureName;
        public Vector2Int gridSize;
        public Vector2Int realFigureSize;
        public Vector3 lastValidPosition;
        public Vector2 colliderSize;
        public bool isActive;
        public Block blockComponent;
        
        public Vector3 PivotToCenterOffset
        {
            get
            {
                if (figureCollider != null && figureTransform != null)
                {
                    return figureCollider.bounds.center - figureTransform.position;
                }
                return Vector3.zero;
            }
        }
    }
    
    private class LayoutCell
    {
        public bool isOccupied;
        public FigureData occupyingFigure;
        public Vector2 worldPosition;
        public Vector2Int gridPosition;
        
        public List<Vector2Int> GetNeighborPositions()
        {
            return new List<Vector2Int>
            {
                new Vector2Int(gridPosition.x - 1, gridPosition.y),
                new Vector2Int(gridPosition.x + 1, gridPosition.y),
                new Vector2Int(gridPosition.x, gridPosition.y - 1),
                new Vector2Int(gridPosition.x, gridPosition.y + 1)
            };
        }
    }
    
    private List<FigureData> allFigures = new List<FigureData>();
    private LayoutCell[,] layoutGrid;
    private Vector2Int layoutGridSize;
    private Vector2 layoutGridOrigin;
    private Transform parentTransform;
    private bool _isInitialized = false;

    private void Awake()
    {
        parentTransform = transform;
    }

    private void Start()
    {
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        AnalyzeAllFigures();
    }

    public void SetOrientation(ScreenOrientation orientation)
    {
        if (currentOrientation == orientation) return;
        
        currentOrientation = orientation;
        
        ArrangeFigures();
    }

    public void AnalyzeAllFigures()
    {
        allFigures.Clear();
        _isInitialized = false;
        
        if (blocks == null || blocks.Length == 0)
        {
            return;
        }
        
        foreach (Block block in blocks)
        {
            if (block == null || block.figureObject == null) continue;
            
            Transform figureTransform = block.figureObject.transform;
            Collider2D figureCollider = figureTransform.GetComponent<Collider2D>();
            
            if (figureCollider == null)
            {
                continue;
            }
            

            
            FigureData figureData = new FigureData();
            figureData.figureTransform = figureTransform;
            figureData.figureCollider = figureCollider;
            figureData.figureName = figureTransform.name;
            figureData.figureColor = GetFigureColor(figureTransform);
            figureData.blockComponent = block;
            figureData.lastValidPosition = figureTransform.localPosition;
            figureData.isActive = figureTransform.gameObject.activeInHierarchy;
            
            Bounds bounds = figureCollider.bounds;
            figureData.colliderSize = bounds.size;
            
            Vector2Int realFigureSize = GetFigureRealSize(block);
            figureData.realFigureSize = realFigureSize;
            
            figureData.gridSize = new Vector2Int(
                Mathf.RoundToInt(bounds.size.x),
                Mathf.RoundToInt(bounds.size.y)
            );
            
            allFigures.Add(figureData);
        }
        
        _isInitialized = true;

        ArrangeFigures();
    }

    private Vector2Int GetFigureRealSize(Block block)
    {
        if (block == null) return Vector2Int.one;

        int polyominoIndex = block.GetPolyominoIndex();
        var polyomino = Polyominus.Get(polyominoIndex);
        
        if (polyomino == null) return Vector2Int.one;
        
        int rows = polyomino.GetLength(0);
        int cols = polyomino.GetLength(1);
        
        int minX = cols, maxX = 0, minY = rows, maxY = 0;
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (polyomino[r, c] > 0)
                {
                    if (c < minX) minX = c;
                    if (c > maxX) maxX = c;
                    if (r < minY) minY = r;
                    if (r > maxY) maxY = r;
                }
            }
        }
        
        return new Vector2Int(maxX - minX + 1, maxY - minY + 1);
    }

    public void RefreshFigurePositions()
    { 
        UpdateFiguresActivity();
        ArrangeFigures();
    }

    private void UpdateFiguresActivity()
    {
        foreach (var figure in allFigures)
        {
            if (figure.figureTransform != null)
            {
                figure.isActive = figure.figureTransform.gameObject.activeInHierarchy;
            }
        }
    }

    private List<FigureData> GetActiveFigures()
    {
        return allFigures.Where(f => f.isActive && f.figureTransform != null).ToList();
    }

    private void ArrangeFigures()
    {
        if (!_isInitialized)
        {
            return;
        }
        
        UpdateFiguresActivity();
        
        var activeFigures = GetActiveFigures();
        
        if (activeFigures.Count == 0)
        {
            return;
        }
        
        Collider2D activeCollider = currentOrientation == ScreenOrientation.Horizontal 
            ? HorizontalCollider 
            : VerticalCollider;
            
        if (activeCollider == null)
        {
            return;
        }
        
        InitializeLayoutGrid(activeCollider);

        var sortedFigures = activeFigures
            .OrderByDescending(f => f.realFigureSize.x * f.realFigureSize.y)
            .ThenByDescending(f => f.realFigureSize.x)
            .ToList();

        if (sortedFigures.Count > 0)
        {
            PlaceFirstFigure(sortedFigures[0]);

            for (int i = 1; i < sortedFigures.Count; i++)
            {
                PlaceFigureInLayout(sortedFigures[i], true);
            }
        }
    }

    private void InitializeLayoutGrid(Collider2D containerCollider)
    {
        Bounds bounds = containerCollider.bounds;
        
        layoutGridSize = new Vector2Int(
            Mathf.FloorToInt(bounds.size.x / cellSize),
            Mathf.FloorToInt(bounds.size.y / cellSize)
        );
        
        layoutGridOrigin = new Vector2(
            bounds.min.x,
            bounds.min.y
        );
        
        layoutGrid = new LayoutCell[layoutGridSize.x, layoutGridSize.y];
        
        for (int x = 0; x < layoutGridSize.x; x++)
        {
            for (int y = 0; y < layoutGridSize.y; y++)
            {
                layoutGrid[x, y] = new LayoutCell
                {
                    isOccupied = false,
                    occupyingFigure = null,
                    gridPosition = new Vector2Int(x, y),
                    worldPosition = new Vector2(
                        layoutGridOrigin.x + (x + 0.5f) * cellSize,
                        layoutGridOrigin.y + (y + 0.5f) * cellSize
                    )
                };
            }
        }
    }

    private void PlaceFirstFigure(FigureData figure)
    {
        for (int y = layoutGridSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < layoutGridSize.x; x++)
            {
                if (CanPlaceFirstFigureAt(x, y, figure))
                {
                    Vector2 targetColliderCenter = CalculateFigureWorldPosition(x, y, figure);
                    
                    Vector3 targetPivotWorld = new Vector3(
                        targetColliderCenter.x - figure.PivotToCenterOffset.x,
                        targetColliderCenter.y - figure.PivotToCenterOffset.y,
                        0
                    );
                    
                    Vector3 targetLocal = parentTransform.InverseTransformPoint(targetPivotWorld);

                    
                    if (figure.blockComponent != null)
                    {
                        figure.blockComponent.SetPosition(targetLocal);
                    }
                    else
                    {
                        figure.figureTransform.localPosition = targetLocal;
                    }
                    figure.lastValidPosition = targetLocal;
                    
                    OccupyCells(x, y, figure);
                    
                    return;
                }
            }
        }
        
    }

    private bool CanPlaceFirstFigureAt(int startX, int startY, FigureData figure)
    {
        Vector2Int figureSize = figure.realFigureSize;
        
        if (startX + figureSize.x > layoutGridSize.x) return false;
        if (startY - figureSize.y + 1 < 0) return false;
        
        for (int x = 0; x < figureSize.x; x++)
        {
            for (int y = 0; y < figureSize.y; y++)
            {
                int checkY = startY - y;
                
                if (layoutGrid[startX + x, checkY].isOccupied)
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    private void PlaceFigureInLayout(FigureData figure, bool checkNeighbors = true)
    {   
        for (int y = layoutGridSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < layoutGridSize.x; x++)
            {
                bool canPlace;
                if (checkNeighbors)
                {
                    canPlace = CanPlaceFigureWithNeighbors(x, y, figure);
                }
                else
                {
                    canPlace = CanPlaceFirstFigureAt(x, y, figure);
                }
                
                if (canPlace)
                {
                    Vector2 targetColliderCenter = CalculateFigureWorldPosition(x, y, figure);
                    
                    Vector3 targetPivotWorld = new Vector3(
                        targetColliderCenter.x - figure.PivotToCenterOffset.x,
                        targetColliderCenter.y - figure.PivotToCenterOffset.y,
                        0
                    );
                    
                    Vector3 targetLocal = parentTransform.InverseTransformPoint(targetPivotWorld);
                    
                    if (figure.blockComponent != null)
                    {
                        figure.blockComponent.SetPosition(targetLocal);
                    }
                    else
                    {
                        figure.figureTransform.localPosition = targetLocal;
                    }
                    figure.lastValidPosition = targetLocal;
                    
                    OccupyCells(x, y, figure);
                    
                    return;
                }
            }
        }
        
        for (int y = layoutGridSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < layoutGridSize.x; x++)
            {
                if (CanPlaceFirstFigureAt(x, y, figure))
                {
                    Vector2 targetColliderCenter = CalculateFigureWorldPosition(x, y, figure);
                    
                    Vector3 targetPivotWorld = new Vector3(
                        targetColliderCenter.x - figure.PivotToCenterOffset.x,
                        targetColliderCenter.y - figure.PivotToCenterOffset.y,
                        0
                    );
                    
                    Vector3 targetLocal = parentTransform.InverseTransformPoint(targetPivotWorld);

                    
                    if (figure.blockComponent != null)
                    {
                        figure.blockComponent.SetPosition(targetLocal);
                    }
                    else
                    {
                        figure.figureTransform.localPosition = targetLocal;
                    }
                    figure.lastValidPosition = targetLocal;
                    
                    OccupyCells(x, y, figure);
                    
                    return;
                }
            }
        }
    }

    private bool CanPlaceFigureWithNeighbors(int startX, int startY, FigureData figure)
    {
        Vector2Int figureSize = figure.realFigureSize;
        
        if (startX + figureSize.x > layoutGridSize.x) return false;
        if (startY - figureSize.y + 1 < 0) return false;

        for (int x = 0; x < figureSize.x; x++)
        {
            for (int y = 0; y < figureSize.y; y++)
            {
                int checkY = startY - y;
                
                if (layoutGrid[startX + x, checkY].isOccupied)
                {
                    return false;
                }
            }
        }

        for (int x = -1; x <= figureSize.x; x++)
        {
            for (int y = -1; y <= figureSize.y; y++)
            {
                if (x >= 0 && x < figureSize.x && y >= 0 && y < figureSize.y)
                    continue;
                
                int checkX = startX + x;
                int checkY = startY - y;
                
                if (checkX >= 0 && checkX < layoutGridSize.x && 
                    checkY >= 0 && checkY < layoutGridSize.y)
                {
                    if (layoutGrid[checkX, checkY].isOccupied)
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }

    private Vector2 CalculateFigureWorldPosition(int startX, int startY, FigureData figure)
    {
        Vector2 topLeftCell = layoutGrid[startX, startY].worldPosition;
        
        float centerX = topLeftCell.x + (figure.realFigureSize.x - 1) * cellSize / 2f;
        float centerY = topLeftCell.y - (figure.realFigureSize.y - 1) * cellSize / 2f;
        
        return new Vector2(centerX, centerY);
    }

    private void OccupyCells(int startX, int startY, FigureData figure)
    {
        for (int x = 0; x < figure.realFigureSize.x; x++)
        {
            for (int y = 0; y < figure.realFigureSize.y; y++)
            {
                int cellY = startY - y;
                layoutGrid[startX + x, cellY].isOccupied = true;
                layoutGrid[startX + x, cellY].occupyingFigure = figure;
            }
        }
    }

    public void OnFiguresRespawned()
    {
        Debug.Log("=== ПЕРЕСПАВН ФИГУР ===");
        AnalyzeAllFigures();
    }

    public void ForceAnalyze()
    {
        AnalyzeAllFigures();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGrid) return;

        DrawColliderBounds(HorizontalCollider, Color.green);
        DrawColliderBounds(VerticalCollider, Color.blue);
        
        Collider2D activeCollider = currentOrientation == ScreenOrientation.Horizontal 
            ? HorizontalCollider 
            : VerticalCollider;

        if (activeCollider != null)
        {
            DrawContainerGrid(activeCollider);
            
            if (layoutGrid != null)
            {
                DrawLayoutGrid();
            }
        }
        
        if (showFigureGrids)
        {
            DrawAllFiguresGrids();
        }
        
        if (showDebugInfo)
        {
            DrawDebugInfo();
            DrawPivotDebug();
        }
    }

    private void DrawLayoutGrid()
    {
        if (layoutGrid == null) return;
        
        for (int x = 0; x < layoutGridSize.x; x++)
        {
            for (int y = 0; y < layoutGridSize.y; y++)
            {
                if (layoutGrid[x, y].isOccupied)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.3f);
                    Gizmos.DrawCube(layoutGrid[x, y].worldPosition, Vector3.one * cellSize * 0.8f);
                }
                else
                {
                    bool hasOccupiedNeighbor = false;
                    foreach (var neighborPos in layoutGrid[x, y].GetNeighborPositions())
                    {
                        if (neighborPos.x >= 0 && neighborPos.x < layoutGridSize.x &&
                            neighborPos.y >= 0 && neighborPos.y < layoutGridSize.y)
                        {
                            if (layoutGrid[neighborPos.x, neighborPos.y].isOccupied)
                            {
                                hasOccupiedNeighbor = true;
                                break;
                            }
                        }
                    }
                    
                    if (hasOccupiedNeighbor)
                    {
                        Gizmos.color = new Color(1, 0.5f, 0, 0.2f);
                        Gizmos.DrawWireCube(layoutGrid[x, y].worldPosition, Vector3.one * cellSize * 0.9f);
                    }
                }
            }
        }
    }

    private void DrawContainerGrid(Collider2D collider)
    {
        Bounds bounds = collider.bounds;
        
        int cellsX = Mathf.FloorToInt(bounds.size.x / cellSize);
        int cellsY = Mathf.FloorToInt(bounds.size.y / cellSize);
        
        Vector2 startPos = new Vector2(bounds.min.x, bounds.min.y);
        
        Gizmos.color = gridColor;
        
        for (int x = 0; x <= cellsX; x++)
        {
            float xPos = startPos.x + x * cellSize;
            Gizmos.DrawLine(
                new Vector3(xPos, startPos.y, 0),
                new Vector3(xPos, startPos.y + cellsY * cellSize, 0)
            );
        }
        
        for (int y = 0; y <= cellsY; y++)
        {
            float yPos = startPos.y + y * cellSize;
            Gizmos.DrawLine(
                new Vector3(startPos.x, yPos, 0),
                new Vector3(startPos.x + cellsX * cellSize, yPos, 0)
            );
        }
    }

    private void DrawPivotDebug()
    {
        if (allFigures == null) return;
        
        foreach (var figure in allFigures)
        {
            if (figure.figureTransform == null || figure.figureCollider == null || !figure.isActive) 
                continue;
            
            Vector3 pivotPos = figure.figureTransform.position;
            Vector3 colliderCenter = figure.figureCollider.bounds.center;
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pivotPos, 0.15f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(colliderCenter, 0.15f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pivotPos, colliderCenter);
        }
    }

    private void DrawDebugInfo()
    {
        if (allFigures == null) return;
        
        foreach (var figure in allFigures)
        {
            if (figure.figureTransform == null || !figure.isActive) continue;
            
            Vector3 worldPos = figure.figureTransform.position;
            
            #if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = figure.isActive ? figure.figureColor : Color.gray;
            style.fontSize = 9;
            
            UnityEditor.Handles.Label(worldPos + Vector3.up * 1.0f, figure.figureName, style);
            #endif
        }
    }

    private void DrawAllFiguresGrids()
    {
        if (blocks == null || blocks.Length == 0) return;
        
        foreach (Block block in blocks)
        {
            if (block == null || block.figureObject == null) continue;
            
            Transform figureTransform = block.figureObject.transform;
            Collider2D figureCollider = figureTransform.GetComponent<Collider2D>();
            
            if (figureCollider == null || !figureTransform.gameObject.activeInHierarchy) 
                continue;
            
            DrawFigureGridFromCollider(figureCollider, GetFigureColor(figureTransform), figureTransform.name);
        }
    }

    private void DrawFigureGridFromCollider(Collider2D figureCollider, Color color, string figureName)
    {
        if (figureCollider == null) return;
        
        Bounds bounds = figureCollider.bounds;
        
        int width = Mathf.RoundToInt(bounds.size.x);
        int height = Mathf.RoundToInt(bounds.size.y);
        
        Vector2 bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        
        Gizmos.color = color;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        Gizmos.color = figureGridColor;
        
        for (int x = 0; x <= width; x++)
        {
            float xPos = bottomLeft.x + x * cellSize;
            Gizmos.DrawLine(
                new Vector3(xPos, bottomLeft.y, 0),
                new Vector3(xPos, bottomLeft.y + height * cellSize, 0)
            );
        }
        
        for (int y = 0; y <= height; y++)
        {
            float yPos = bottomLeft.y + y * cellSize;
            Gizmos.DrawLine(
                new Vector3(bottomLeft.x, yPos, 0),
                new Vector3(bottomLeft.x + width * cellSize, yPos, 0)
            );
        }
    }

    private void DrawColliderBounds(Collider2D collider, Color color)
    {
        if (collider == null) return;
        
        color.a = 0.1f;
        Gizmos.color = color;
        Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
    }
    
    private Color GetFigureColor(Transform figure)
    {
        if (figure == null) return Color.red;
        
        int hash = figure.name.GetHashCode();
        float r = (hash & 0xFF) / 255f;
        float g = ((hash >> 8) & 0xFF) / 255f;
        float b = ((hash >> 16) & 0xFF) / 255f;
        return new Color(r, g, b);
    }

    [ContextMenu("Analyze Figures")]
    public void AnalyzeFigures()
    {
        AnalyzeAllFigures();
    }
    
    [ContextMenu("Arrange Figures")]
    private void ArrangeFiguresManual()
    {
        ArrangeFigures();
    }
}