using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldRadialFill : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private Image radialImage;

    [Header("Настройки")]
    [SerializeField] private float fillSpeed = 1f;
    [SerializeField] private UnityEvent onFillComplete;

    private bool isHolding = false;
    private float currentFill = 0f;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (radialImage == null)
            radialImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            rectTransform.position = Input.mousePosition;

            if (!isHolding)
            {
                ShowRadial();
            }

            currentFill += fillSpeed * Time.deltaTime;
            currentFill = Mathf.Clamp01(currentFill);
            radialImage.fillAmount = currentFill;

            if (currentFill >= 1f)
            {
                onFillComplete?.Invoke();
            }
        }
        else
        {
            if (isHolding)
            {
                HideRadial();
            }
        }
    }

    private void ShowRadial()
    {
        gameObject.GetComponent<Image>().enabled = true;
        isHolding = true;
        currentFill = 0f;
        radialImage.fillAmount = 0f;
    }

    private void HideRadial()
    {
        gameObject.GetComponent<Image>().enabled = false;
        isHolding = false;
        currentFill = 0f;
        radialImage.fillAmount = 0f;
    }

    public void ForceComplete()
    {
        if (isHolding)
        {
            onFillComplete?.Invoke();
            HideRadial();
        }
    }
}