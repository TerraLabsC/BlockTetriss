using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public bool useMainCamera = true;
    public Camera targetCamera;

    [Space]
    public float duration = 1.0f;
    public Vector3 shakeStrength = new Vector3(0.1f, 0.1f, 0.1f);
    public AnimationCurve shakeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    [Range(0, 0.1f)] public float shakesDelay = 0;

    private Vector3 originalPosition;
    private bool isShaking = false;
    private float shakeTimer = 0f;
    private float delayTimer = 0f;

    public Orientation orientation;

    void Awake()
    {
        orientation = GetComponent<Orientation>();
        if (orientation != null)
        {
            orientation.OnOrientationChanged += OnOrientationChanged;
        }
    }

    void Start()
    {
        InitializeCamera();
    }

    void InitializeCamera()
    {
        if (useMainCamera)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("Main Camera not found!");
                enabled = false;
                return;
            }
        }

        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
            {
                Debug.LogError("No camera assigned!");
                enabled = false;
                return;
            }
        }

        if (orientation != null)
        {
            Vector3 initialPos = orientation.IsHorizontal
                ? orientation.PositionCameraHorizontal
                : orientation.PositionCameraVertical;
            SetBasePosition(initialPos);
        }
        else
        {
            SetBasePosition(targetCamera.transform.position);
        }
    }

    private void OnOrientationChanged(Vector3 newBasePosition)
    {
        if (isShaking)
        {
            Vector3 currentOffset = targetCamera.transform.position - originalPosition;
            SetBasePosition(newBasePosition);

            targetCamera.transform.position = originalPosition + currentOffset;
        }
        else
        {
            SetBasePosition(newBasePosition);
        }
    }

    private void SetBasePosition(Vector3 newBase)
    {
        originalPosition = newBase;
    }

    public void ShakeCamera()
    {
        isShaking = true;
        shakeTimer = 0f;
        delayTimer = 0f;
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        while (isShaking)
        {
            yield return null;

            if (shakeTimer >= duration)
            {
                StopShake();
                yield break;
            }

            if (shakesDelay > 0)
            {
                delayTimer += Time.deltaTime;
                if (delayTimer < shakesDelay)
                {
                    continue;
                }
                else
                {
                    delayTimer = 0f;
                }
            }

            float progress = shakeTimer / duration;
            float curveValue = shakeCurve.Evaluate(progress);

            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f) * shakeStrength.x,
                Random.Range(-1f, 1f) * shakeStrength.y,
                Random.Range(-1f, 1f) * shakeStrength.z
            ) * curveValue;

            targetCamera.transform.position = originalPosition + randomOffset;
            shakeTimer += Time.deltaTime;
        }
    }

    public void StopShake()
    {
        isShaking = false;
        StopAllCoroutines();
        targetCamera.transform.position = originalPosition;
    }

    void OnDisable()
    {
        StopShake();
        if (orientation != null)
            orientation.OnOrientationChanged -= OnOrientationChanged;
    }
}