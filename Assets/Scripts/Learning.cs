using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Learning : MonoBehaviour
{
    [SerializeField] private GameObject[] Blocks;
    [SerializeField] private GameObject[] PositionBlock;
    [SerializeField] private float SpeedBlocks = 5f;
    [SerializeField] private UnityEvent OnAllBlocksArrived;
    [SerializeField] private UnityEvent OnTimer;

    [Header("Дополнительные настройки")]
    [SerializeField] private bool useSmoothDamp = false;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float delayBetweenBlocks = 0.5f;
    [SerializeField] private int batchSize = 3;

    [Header("Настройки родителя")]
    [SerializeField] private bool detachFromParentOnArrival = true;
    [SerializeField] private Transform newParent;

    private int currentBatchIndex = 0;
    private int totalBlocksMoved = 0;
    private float arrivalThreshold = 0.1f;
    private Vector3[] velocities;
    private List<int> movingBlocksInBatch = new List<int>();
    private Transform originalParent;

    [SerializeField] private GameObject[] DestroyObject;

    [SerializeField] private GameObject BlockObjectLearning;
    [SerializeField] private Vector3 PositionLearningHorizontal;
    [SerializeField] private Vector3 PositionLearningVertical;
    private void Update()
    {
        if (Screen.width > Screen.height)
        {
            BlockObjectLearning.transform.position = PositionLearningHorizontal;
        }
        else
        {
            BlockObjectLearning.transform.position = PositionLearningVertical;
        }
    }


    void Start()
    {
        if (Blocks == null || PositionBlock == null || Blocks.Length != PositionBlock.Length)
        {
            Debug.LogError("Массивы должны быть одинаковой длины!");
            return;
        }

        velocities = new Vector3[Blocks.Length];

        if (Blocks.Length > 0 && Blocks[0] != null)
        {
            originalParent = Blocks[0].transform.parent;
        }

        DeactivateAllBlocks();

        StartCoroutine(MoveSequence());
    }
    private void DeactivateAllBlocks()
    {
        foreach (GameObject block in Blocks)
        {
            if (block != null)
            {
                block.SetActive(false);
            }
        }
    }

    public void DeactivateBlocks()
    {
        foreach (GameObject block in DestroyObject)
        {
            if (block != null)
            {
                block.SetActive(false);
            }
        }
    }
    private void DetachBlockFromParent(GameObject block)
    {
        if (block == null) return;

        Transform currentParent = block.transform.parent;

        if (currentParent == null || !detachFromParentOnArrival) return;

        Debug.Log($"Открепляем блок {block.name} от родителя {currentParent.name}");

        Vector3 worldPosition = block.transform.position;
        Quaternion worldRotation = block.transform.rotation;
        Vector3 worldScale = block.transform.lossyScale;

        if (newParent != null)
        {
            block.transform.SetParent(newParent, false);
            Debug.Log($"Блок {block.name} прикреплен к новому родителю {newParent.name}");
        }
        else
        {
            block.transform.SetParent(null, false);
            Debug.Log($"Блок {block.name} откреплен от родителя (теперь сирота)");
        }

        block.transform.position = worldPosition;
        block.transform.rotation = worldRotation;
    }

    IEnumerator MoveSequence()
    {
        int totalBatches = Mathf.CeilToInt((float)Blocks.Length / batchSize);

        for (int batch = 0; batch < totalBatches; batch++)
        {
            int startIndex = batch * batchSize;
            int endIndex = Mathf.Min(startIndex + batchSize, Blocks.Length);

            for (int i = startIndex; i < endIndex; i++)
            {
                if (Blocks[i] != null && PositionBlock[i] != null)
                {
                    Blocks[i].SetActive(true);
                    Debug.Log($"Активирован блок {i} (группа {batch + 1})");
                }
            }

            movingBlocksInBatch.Clear();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (Blocks[i] == null || PositionBlock[i] == null)
                {
                    Debug.LogError($"Блок или позиция {i} не назначены!");
                    continue;
                }

                movingBlocksInBatch.Add(i);

                StartCoroutine(MoveBlockInBatch(i));

                if (i < endIndex - 1)
                {
                    yield return new WaitForSeconds(delayBetweenBlocks);
                }
            }

            yield return new WaitUntil(() => movingBlocksInBatch.Count == 0);

            Debug.Log($"Группа {batch + 1} завершила движение");
        }

        StartCoroutine(EventActive());
        Debug.Log("Все блоки достигли своих позиций!");
    }

    public IEnumerator EventActive() 
    {
        yield return new WaitForSeconds(0.5f);
        OnAllBlocksArrived?.Invoke();
        yield return new WaitForSeconds(1f);
        OnTimer?.Invoke();
    }

    IEnumerator MoveBlockInBatch(int index)
    {
        GameObject block = Blocks[index];

        yield return new WaitForSeconds(0.5f);

        if (!block.activeSelf)
        {
            Debug.LogWarning($"Блок {index} не активен, пропускаем движение");
            movingBlocksInBatch.Remove(index);
            yield break;
        }

        Vector3 targetPos = PositionBlock[index].transform.position;
        Vector3 startPos = block.transform.position;
        float journeyLength = Vector3.Distance(startPos, targetPos);
        float startTime = Time.time;

        Debug.Log($"Блок {index} начал движение к позиции {targetPos}");

        while (Vector3.Distance(block.transform.position, targetPos) > arrivalThreshold)
        {
            if (!block.activeSelf)
            {
                Debug.Log($"Блок {index} был деактивирован во время движения");
                movingBlocksInBatch.Remove(index);
                yield break;
            }

            float distCovered = (Time.time - startTime) * SpeedBlocks;
            float fractionOfJourney = journeyLength > 0 ? distCovered / journeyLength : 1f;

            if (useSmoothDamp)
            {
                block.transform.position = Vector3.SmoothDamp(
                    block.transform.position,
                    targetPos,
                    ref velocities[index],
                    smoothTime,
                    SpeedBlocks
                );
            }
            else
            {
                float curveValue = moveCurve.Evaluate(Mathf.Clamp01(fractionOfJourney));
                block.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            }

            yield return null;
        }

        block.transform.position = targetPos;

        DetachBlockFromParent(block);

        movingBlocksInBatch.Remove(index);

        block.gameObject.GetComponent<ScaleObjectDotWeen>().ScaleUpAndDown();

        totalBlocksMoved++;
        Debug.Log($"Блок {index} достиг цели и откреплен от родителя. Всего блоков перемещено: {totalBlocksMoved}/{Blocks.Length}");
    }

    public void StartMovement()
    {
        StopAllCoroutines();
        currentBatchIndex = 0;
        totalBlocksMoved = 0;
        movingBlocksInBatch.Clear();

        DeactivateAllBlocks();

        StartCoroutine(MoveSequence());
    }

    public void StopMovement()
    {
        StopAllCoroutines();
    }

    public void ActivateAllBlocks()
    {
        foreach (GameObject block in Blocks)
        {
            if (block != null)
            {
                block.SetActive(true);
            }
        }
    }
    public void ActivateBlock(int index)
    {
        if (index >= 0 && index < Blocks.Length && Blocks[index] != null)
        {
            Blocks[index].SetActive(true);
        }
    }
    public void DeactivateBlock(int index)
    {
        if (index >= 0 && index < Blocks.Length && Blocks[index] != null)
        {
            Blocks[index].SetActive(false);
        }
    }

    public void SetBatchSize(int newSize)
    {
        if (newSize > 0)
        {
            batchSize = newSize;
        }
    }

    public void DetachAllBlocks()
    {
        foreach (GameObject block in Blocks)
        {
            if (block != null)
            {
                DetachBlockFromParent(block);
            }
        }
    }

    public void ReattachToOriginalParent()
    {
        if (originalParent == null) return;

        foreach (GameObject block in Blocks)
        {
            if (block != null)
            {
                Vector3 worldPosition = block.transform.position;
                Quaternion worldRotation = block.transform.rotation;

                block.transform.SetParent(originalParent, false);

                block.transform.position = worldPosition;
                block.transform.rotation = worldRotation;

                Debug.Log($"Блок {block.name} возвращен к оригинальному родителю");
            }
        }
    }
}