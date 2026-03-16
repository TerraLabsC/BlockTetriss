using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldRadialFill : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private Image radialImage; // Круг с Filled -> Radial 360

    [Header("Настройки")]
    [SerializeField] private float fillSpeed = 1f; // Скорость заполнения
    [SerializeField] private UnityEvent onFillComplete; // Событие при полном заполнении

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
        // Проверяем зажата ли левая кнопка мыши
        if (Input.GetMouseButton(0)) // 0 - левая кнопка
        {
            // Двигаем круг за мышкой
            rectTransform.position = Input.mousePosition;

            // Если только начали зажимать - показываем круг
            if (!isHolding)
            {
                ShowRadial();
            }

            // Увеличиваем заполнение
            currentFill += fillSpeed * Time.deltaTime;
            currentFill = Mathf.Clamp01(currentFill);
            radialImage.fillAmount = currentFill;

            // Проверяем на полное заполнение
            if (currentFill >= 1f)
            {
                onFillComplete?.Invoke();
                // Можно либо скрыть, либо оставить для следующего нажатия
                // HideRadial(); // Раскомментируйте если нужно скрывать после завершения
            }
        }
        else // Кнопка не зажата
        {
            // Если держали и отпустили - скрываем и обнуляем
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

    // Публичный метод для принудительного завершения
    public void ForceComplete()
    {
        if (isHolding)
        {
            onFillComplete?.Invoke();
            HideRadial();
        }
    }
}