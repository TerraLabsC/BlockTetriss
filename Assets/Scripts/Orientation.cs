using UnityEngine;

public class Orientation : MonoBehaviour
{
    private Camera _camera;
    private bool _isHorizontal = false;
    private bool _isInitialized = false;

    [SerializeField] private Canvas CanvasHorizontal;
    [SerializeField] private Canvas CanvasVertical;
    [SerializeField] private float CameraFOVHorizontal;
    [SerializeField] private float CameraFOVVertical;
    [SerializeField] private Vector3 PositionCameraHorizontal;
    [SerializeField] private Vector3 PositionCameraVertical;
    [SerializeField] private PositionPolyominus blocks;
    [SerializeField] private GameObject PanelHorizontalPolyominus;
    [SerializeField] private GameObject PanelVerticalPolyominus;
    [SerializeField] private GameObject CatelHorizontalPolyominus;
    [SerializeField] private GameObject CatelVerticalPolyominus;

    private Vector3 PositionCateHorizontal;
    private Vector3 PositionCateVertical;

    [SerializeField] private Vector3 PositionCateMove;
    [SerializeField] private GameObject CountpPanel;
    [SerializeField] private Vector3 PositionCountHorizontal;
    [SerializeField] private Vector3 PositionCountVertical;

    private void Start()
    {
        PositionCateHorizontal = CatelHorizontalPolyominus.transform.position;
        PositionCateVertical = CatelVerticalPolyominus.transform.position;

        _camera = Camera.main;
        Time.timeScale = 1f;

        UpdateOrientation(true);
    }

    private void Update()
    {
        if (_camera == null) return;

        bool shouldBeHorizontal = Screen.width > Screen.height;

        if (shouldBeHorizontal != _isHorizontal || !_isInitialized)
        {
            UpdateOrientation(shouldBeHorizontal);
        }
    }

    private void UpdateOrientation(bool isHorizontal)
    {
        _isHorizontal = isHorizontal;
        _isInitialized = true;

        if (isHorizontal)
        {
            Debug.Log("Переключение на горизонтальный режим");
            
            _camera.orthographicSize = CameraFOVHorizontal;
            _camera.transform.position = PositionCameraHorizontal;

            CanvasHorizontal.enabled = true;
            CanvasVertical.enabled = false;

            PanelHorizontalPolyominus.SetActive(true);
            PanelVerticalPolyominus.SetActive(false);

            CatelHorizontalPolyominus.transform.position = PositionCateHorizontal;
            CatelVerticalPolyominus.transform.position = PositionCateMove;
            
            blocks.SetOrientation(ScreenOrientation.Vertical);

            CountpPanel.transform.position = PositionCountHorizontal;
        }
        else
        {
            Debug.Log("Переключение на вертикальный режим");
            
            _camera.orthographicSize = CameraFOVVertical;
            _camera.transform.position = PositionCameraVertical;

            CanvasHorizontal.enabled = false;
            CanvasVertical.enabled = true;

            PanelHorizontalPolyominus.SetActive(false);
            PanelVerticalPolyominus.SetActive(true);
            CatelHorizontalPolyominus.transform.position = PositionCateMove;
            CatelVerticalPolyominus.transform.position = PositionCateVertical;

            blocks.SetOrientation(ScreenOrientation.Horizontal);

            CountpPanel.transform.position = PositionCountVertical;
        }
    }
}