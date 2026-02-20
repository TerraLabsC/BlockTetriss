using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour
{
    public const int Size = 5;

    private readonly Vector3 inputOffSet = new(0.0f, 2.0f, 0.0f);

    [SerializeField] private Board board;
    [SerializeField] private Blocks blocks;

    [SerializeField] private Cell cellPrefab;

    private int polyominoIndex;

    private readonly Cell[,] cells = new Cell[Size, Size];

    private Vector3 position;
    private Vector3 scale;

    private Vector2 inputPoint;

    private Vector3 previousMousePosition = Vector3.positiveInfinity;

    private Vector2Int previousDragPoint;
    private Vector2Int currentDragPoint;

    private Camera mainCamera;

    private Vector2 center;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize()
    {
        for (var r = 0; r < Size; ++r)
        {
            for (var c = 0; c < Size; ++c)
            {
                cells[r,c] = Instantiate(cellPrefab, transform);
            }
        }

        position = transform.localPosition;

        scale = transform.localScale;
    }

    public void Show(int polyominoIndex)
    {
        this.polyominoIndex = polyominoIndex;
        Hide();

        var polyomino  = Polyominus.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var poluominoColumns = polyomino.GetLength(1);
        center = new Vector2(poluominoColumns * 0.5f, polyominoRows * 0.5f);

        for (var r = 0; r < polyominoRows; ++r)
        {
            for (var c = 0; c < poluominoColumns; ++c)
            {
                if (polyomino[r,c] > 0)
                {
                    cells[r, c].transform.localPosition = new(c - center.x + 0.5f, r - center.y + 0.5f, 0.0f);
                    cells[r, c].Normal();
                }
            }
        }
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

    private void OnMouseDown()
    {
        inputPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        transform.localPosition = position + inputOffSet;
        transform.localScale = Vector3.one;

        currentDragPoint = Vector2Int.RoundToInt((Vector2)transform.position - center);
        board.Hover(currentDragPoint, polyominoIndex);
        previousDragPoint = currentDragPoint;

        previousMousePosition = Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        var currentMousePosition = Input.mousePosition;

        if (currentMousePosition != previousMousePosition)
        {
            previousMousePosition = currentMousePosition;

            var inputDelta = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition) - inputPoint;
            transform.localPosition = position + inputOffSet + (Vector3)inputDelta * 1.4f;

            currentDragPoint = Vector2Int.RoundToInt((Vector2)transform.position - center);

            if(currentDragPoint != previousDragPoint)
            {
                previousDragPoint = currentDragPoint;
                board.Hover(currentDragPoint, polyominoIndex);
            }
        }
    }

    private void OnMouseUp()
    {
        previousMousePosition = Vector3.positiveInfinity;

        currentDragPoint = Vector2Int.RoundToInt((Vector2)transform.position - center);

        if(board.Place(currentDragPoint, polyominoIndex) == true)
        {
            gameObject.SetActive(false);

            blocks.Remove();
        }
            
        transform.localPosition = position;
        transform.localScale = scale;


    }
}
