using UnityEngine;

public class OrientationMenu : MonoBehaviour
{
    [SerializeField] private Canvas CanvasHorizontal;

    [SerializeField] private Canvas CanvasVertical;
    
    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Screen.width > Screen.height)
        {
            CanvasHorizontal.enabled = true;
            CanvasVertical.enabled = false;
        }
        else
        {
            CanvasHorizontal.enabled = false;
            CanvasVertical.enabled = true;
        }
    }
}
