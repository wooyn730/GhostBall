using UnityEngine;
using System.Collections;

public class MergeManager : MonoBehaviour
{
    [SerializeField] private GameObject mergeEffect;
    private Coroutine mergeEffectCoroutine;
    private Coroutine trySummonCoroutine;

    public void StartMerge(DragRotateController redDrag, DragRotateController blueDrag)
    {
        if (mergeEffectCoroutine != null) StopCoroutine(mergeEffectCoroutine);
        mergeEffectCoroutine = StartCoroutine(MergeEffect(redDrag, blueDrag));
    }

    private IEnumerator MergeEffect(DragRotateController redDrag, DragRotateController blueDrag)
    {
        mergeEffect.SetActive(true);
        float mergeDistance = 0.1f;
        if (redDrag != null) redDrag.StartMergeSpin(180f);
        if (blueDrag != null) blueDrag.StartMergeSpin(180f);
        Vector3 redPos, bluePos, middle;
        while (true)
        {
            redPos = redDrag.targetObject.transform.position;
            bluePos = blueDrag.targetObject.transform.position;
            middle = (redPos + bluePos) * 0.5f;
            float dist = Vector3.Distance(redPos, bluePos);
            if (mergeEffect != null) mergeEffect.transform.position = middle;
            if (dist <= mergeDistance) break;
            yield return null;
        }
        mergeEffect.SetActive(false);
        if (redDrag != null) redDrag.StopMergeSpin();
        if (blueDrag != null) blueDrag.StopMergeSpin();
        Vector3 dir = (bluePos - redPos).normalized;
        float offset = 0.025f;
        redDrag.targetObject.transform.position = middle - dir * offset;
        blueDrag.targetObject.transform.position = middle + dir * offset;
        mergeEffectCoroutine = null;
        if (trySummonCoroutine != null) StopCoroutine(trySummonCoroutine);
        trySummonCoroutine = StartCoroutine(TrySummonLoop());
        // 머지 후 카드 상태를 None으로 초기화
        if (redDrag != null && redDrag.card != null) redDrag.card.CurrentMotionState = Card.MotionState.None;
        if (blueDrag != null && blueDrag.card != null) blueDrag.card.CurrentMotionState = Card.MotionState.None;
    }

    private IEnumerator TrySummonLoop()
    {
        while (true)
        {
            TrySummon();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void TrySummon()
    {
        // TODO: 동작 구현
    }
} 