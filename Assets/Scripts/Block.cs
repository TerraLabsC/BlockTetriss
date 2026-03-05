using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour
{
    public const int Size = 5;
    public GameObject figureObject;
    [SerializeField] private Board board;
    [SerializeField] private Blocks blocks;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private BoxCollider2D boxCollider;
    private int polyominoIndex;
    private int colorIndex;
    private readonly Cell[,] cells = new Cell[Size, Size];

    private Vector3 position;
    private Vector3 scale;
    private Vector2 offset;
    private Vector3 previousMousePosition = Vector3.positiveInfinity;
    private Vector2Int previousDragPoint;
    private Vector2Int currentDragPoint;
    private Camera mainCamera;
    private Vector2 center;

    public GameObject Taining;
    public GameObject TainingFinger;
    
    private bool hasBeenPositioned = false;

    [SerializeField] private float dragOffsetDistance = 3f;
    [SerializeField] private Vector2 dragOffsetDirection = new Vector2(-0.5f, 0.5f);
    [SerializeField] private float colliderScaleFactor = 1f;

    private int currentPolyominoIndex;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
    }

    public void Initialize()
    {
        for (var r = 0; r < Size; ++r)
        {
            for (var c = 0; c < Size; ++c)
            {
                cells[r, c] = Instantiate(cellPrefab, transform);
            }
        }
    }

    public void UpdatePositionData()
    {
        position = transform.localPosition;
        scale = transform.localScale;
        hasBeenPositioned = true;
    }

    public Vector2Int GetBounds()
    {
        var polyomino = Polyominus.Get(polyominoIndex);
        var rows = polyomino.GetLength(0);
        var cols = polyomino.GetLength(1);
        
        int minX = cols, maxX = 0, minY = rows, maxY = 0;
        
        for (var r = 0; r < rows; ++r)
        {
            for (var c = 0; c < cols; ++c)
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

    public void Show(int polyominoIndex)
    {
        this.polyominoIndex = polyominoIndex;
        this.currentPolyominoIndex = polyominoIndex;
        colorIndex = UnityEngine.Random.Range(0, 6);
        Hide();
        hasBeenPositioned = false;
        
        var polyomino = Polyominus.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var polyominoColumns = polyomino.GetLength(1);

        float sumX = 0, sumY = 0;
        int count = 0;
        for (var r = 0; r < polyominoRows; ++r)
        {
            for (var c = 0; c < polyominoColumns; ++c)
            {
                if (polyomino[r, c] > 0)
                {
                    sumX += c;
                    sumY += r;
                    ++count;
                }
            }
        }
        center = new Vector2(sumX / count, sumY / count);

        for (var r = 0; r < polyominoRows; ++r)
        {
            for (var c = 0; c < polyominoColumns; ++c)
            {
                if (polyomino[r, c] > 0)
                {
                    cells[r, c].transform.localPosition = new Vector3(
                        c - center.x,
                        r - center.y,
                        0
                    );
                    cells[r, c].SetColor(colorIndex);
                    cells[r, c].Normal();
                }
            }
        }
        
        UpdateCollider();
    }

    private void Hide()
    {
        for (var r = 0; r < Size; ++r)
        {
            for (var c = 0; c < Size; ++c)
            {
                cells[r, c].Hide();
            }
        }
    }

    public Vector2 GetColliderSize()
    {
        return boxCollider.size;
    }

    private void OnMouseDown()
    {
        SetChildSortingOrder(9);

        var mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        offset = (Vector2)transform.position - (Vector2)mouseWorldPos;
        
        transform.localScale = Vector3.one;
        
        UpdateDragPoint();
        board.Hover(currentDragPoint, polyominoIndex, colorIndex);
        previousDragPoint = currentDragPoint;
        
        OffsetFigureForDrag();

        if(Taining != null && TainingFinger != null)
        {
            Taining.SetActive(false);
            TainingFinger.SetActive(false);
        }
    }
    private void OffsetFigureForDrag()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        Vector3 offsetPosition = mouseWorldPos + new Vector3(
            dragOffsetDirection.x * dragOffsetDistance,
            dragOffsetDirection.y * dragOffsetDistance,
            0
        );

        transform.position = offsetPosition;

        offset = (Vector2)transform.position - (Vector2)mouseWorldPos;
    }

    private void SetChildSortingOrder(int order)
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.sortingOrder = order;
        }
    }

    private void OnMouseDrag()
    {
        var currentMousePosition = Input.mousePosition;
        
        if (currentMousePosition != previousMousePosition)
        {
            previousMousePosition = currentMousePosition;
            
            var mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = (Vector3)((Vector2)mouseWorldPos + offset);
            
            UpdateDragPoint();
            
            if (currentDragPoint != previousDragPoint)
            {
                previousDragPoint = currentDragPoint;
                board.Hover(currentDragPoint, polyominoIndex, colorIndex);
            }
        }
    }

    private void OnMouseUp()
    {
        SetChildSortingOrder(8);

        previousMousePosition = Vector3.positiveInfinity;
        
        UpdateDragPoint();
        
        if (board.Place(currentDragPoint, polyominoIndex, colorIndex))
        {
            gameObject.SetActive(false);
            blocks.Remove();
        }
        else
        {
            if (hasBeenPositioned)
            {
                transform.localPosition = position;
                transform.localScale = scale;
            }
        }
    }

    private void UpdateDragPoint()
    {
        currentDragPoint = Vector2Int.RoundToInt((Vector2)transform.position - center - new Vector2(0.5f, 0.5f));
    }

    private void UpdateCollider()
    {
         var polyomino = Polyominus.Get(polyominoIndex);
        var rows = polyomino.GetLength(0);
        var cols = polyomino.GetLength(1);
        
        int minX = cols, maxX = 0, minY = rows, maxY = 0;
        bool hasActiveCells = false;
        
        for (var r = 0; r < rows; ++r)
        {
            for (var c = 0; c < cols; ++c)
            {
                if (polyomino[r, c] > 0)
                {
                    hasActiveCells = true;
                    if (c < minX) minX = c;
                    if (c > maxX) maxX = c;
                    if (r < minY) minY = r;
                    if (r > maxY) maxY = r;
                }
            }
        }
        
        if (hasActiveCells)
        {
            float figureCenterX = (minX + maxX) / 2f;
            float figureCenterY = (minY + maxY) / 2f;

            float width = maxX - minX + 1;
            float height = maxY - minY + 1;
            
            float scaledWidth = width + colliderScaleFactor;
            float scaledHeight = height + colliderScaleFactor;
            
            boxCollider.size = new Vector2(scaledWidth, scaledHeight);

            boxCollider.offset = new Vector2(
                figureCenterX - center.x,
                figureCenterY - center.y
            );
            
            Debug.Log($"Коллайдер фигуры {name}: базовый размер {width}x{height}, увеличен до {scaledWidth}x{scaledHeight}");
        }
    }
    
    public void SetPosition(Vector3 newLocalPosition)
    {
        transform.localPosition = newLocalPosition;
        UpdatePositionData();
    }
    
    public int GetPolyominoIndex()
    {
        return currentPolyominoIndex;
    }
}