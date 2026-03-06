using UnityEngine;
using DG.Tweening;

public class ScaleObjectDotWeen : MonoBehaviour
{
    [Header("Настройки масштабирования")]
    [SerializeField] private float targetScale = 2f;
    [SerializeField] private float scaleUpDuration = 0.5f;
    [SerializeField] private float delayBeforeScaleDown = 1f;
    [SerializeField] private float scaleDownDuration = 0.5f;
    [SerializeField] private bool scaleOnStart = true;

    private Vector3 originalScale;
    private Tween scaleTween;

    void Start()
    {
        originalScale = transform.localScale;

        if (scaleOnStart)
        {
            ScaleUpAndDown();
        }
    }

    /// <summary>
    /// Увеличивает объект, а затем уменьшает его обратно
    /// </summary>
    public void ScaleUpAndDown()
    {
        scaleTween?.Kill();

        Sequence scaleSequence = DOTween.Sequence();

        scaleSequence.Append(transform.DOScale(targetScale, scaleUpDuration).SetEase(Ease.OutBack));

        scaleSequence.AppendInterval(delayBeforeScaleDown);

        scaleSequence.Append(transform.DOScale(originalScale, scaleDownDuration).SetEase(Ease.InBack));

        scaleTween = scaleSequence;
    }

    /// <summary>
    /// Увеличивает объект до указанного размера
    /// </summary>
    public void ScaleUp(float customScale = 0)
    {
        scaleTween?.Kill();

        float scale = customScale > 0 ? customScale : targetScale;
        scaleTween = transform.DOScale(scale, scaleUpDuration)
            .SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Уменьшает объект до оригинального размера
    /// </summary>
    public void ScaleDown()
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, scaleDownDuration)
            .SetEase(Ease.InBack);
    }

    /// <summary>
    /// Увеличивает объект и автоматически уменьшает через указанную задержку
    /// </summary>
    public void ScaleUpAndDownWithDelay(float upScale, float upDuration, float delay, float downDuration)
    {
        scaleTween?.Kill();

        Sequence customSequence = DOTween.Sequence();
        customSequence.Append(transform.DOScale(upScale, upDuration).SetEase(Ease.OutBack));
        customSequence.AppendInterval(delay);
        customSequence.Append(transform.DOScale(originalScale, downDuration).SetEase(Ease.InBack));

        scaleTween = customSequence;
    }

    /// <summary>
    /// Немедленно останавливает все анимации масштабирования
    /// </summary>
    public void StopScaling()
    {
        scaleTween?.Kill();
    }

    /// <summary>
    /// Сбрасывает масштаб до оригинального
    /// </summary>
    public void ResetScale()
    {
        StopScaling();
        transform.localScale = originalScale;
    }

    void OnDestroy()
    {
        scaleTween?.Kill();
    }
}