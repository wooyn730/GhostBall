using UnityEngine;
using System.Collections;

public class TapRotateController : MonoBehaviour
{
    [SerializeField] private Card card;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject sparkEffect;
    [SerializeField] private GameObject orbEffect;
    [SerializeField] private GameObject otherCard;
    private bool isRotating = false;
    private Vector2 tapStartPos;
    private float tapStartTime;
    private const float dragThreshold = 10f;
    private const float timeThreshold = 0.2f;
    private float lastTapTime = 0f;
    private const float doubleTapMaxDelay = 0.3f;
    private bool waitingForSecondTap = false;

    private void Update()
    {
        if (isRotating)
            return;
        if (card != null && card.CurrentMotionState == Card.MotionState.DoubleTap)
            return;
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            tapStartPos = Input.mousePosition;
            tapStartTime = Time.time;
        }
        if (Input.GetMouseButtonUp(0))
        {
            float tapDist = (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(tapStartPos.x, tapStartPos.y)).magnitude;
            float tapTime = Time.time - tapStartTime;
            if (tapDist <= dragThreshold && tapTime <= timeThreshold)
            {
                if (!waitingForSecondTap)
                {
                    // 첫 번째 탭 Up
                    waitingForSecondTap = true;
                    lastTapTime = Time.time;
                    StartCoroutine(SingleTapDelay());
                }
                else
                {
                    // 두 번째 탭 Up (더블탭)
                    waitingForSecondTap = false;
                    StopAllCoroutines(); // 단일탭 대기 취소
                    if (!isRotating)
                        StartCoroutine(RotateOnly(targetObject));
                }
            }
        }
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                tapStartPos = t.position;
                tapStartTime = Time.time;
            }
            if (t.phase == TouchPhase.Ended)
            {
                float tapDist = (t.position - tapStartPos).magnitude;
                float tapTime = Time.time - tapStartTime;
                if (tapDist <= dragThreshold && tapTime <= timeThreshold)
                {
                    if (!waitingForSecondTap)
                    {
                        // 첫 번째 탭 Up
                        waitingForSecondTap = true;
                        lastTapTime = Time.time;
                        StartCoroutine(SingleTapDelay());
                    }
                    else
                    {
                        // 두 번째 탭 Up (더블탭)
                        waitingForSecondTap = false;
                        StopAllCoroutines(); // 단일탭 대기 취소
                        if (!isRotating)
                            StartCoroutine(RotateOnly(targetObject));
                    }
                }
            }
        }
#endif
    }

    private IEnumerator RotateAndReturn(GameObject targetObject)
    {
        isRotating = true;
        if (card != null)
            card.CurrentMotionState = Card.MotionState.Tap;
        if (sparkEffect != null)
            sparkEffect.SetActive(true);
        
        Quaternion startRotation = targetObject.transform.rotation;
        Quaternion rotated = startRotation * Quaternion.Euler(-90f, 0, 0);
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            targetObject.transform.rotation = Quaternion.Slerp(startRotation, rotated, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        targetObject.transform.rotation = rotated;

        yield return new WaitForSeconds(3f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            targetObject.transform.rotation = Quaternion.Slerp(rotated, startRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        targetObject.transform.rotation = startRotation;
        isRotating = false;
        if (card != null)
            card.CurrentMotionState = Card.MotionState.None;
        if (sparkEffect != null)
            sparkEffect.SetActive(false);
    }

    // 더블탭 시 90도 회전만 하고 복귀하지 않는 코루틴
    private IEnumerator RotateOnly(GameObject targetObject)
    {
        isRotating = true;
        if (card != null)
            card.CurrentMotionState = Card.MotionState.DoubleTap;
        if (sparkEffect != null)
            sparkEffect.SetActive(true);
        if (orbEffect != null)
            orbEffect.SetActive(true);

        Quaternion startRotation = targetObject.transform.rotation;
        Quaternion rotated = startRotation * Quaternion.Euler(-90f, 0, 0);
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            targetObject.transform.rotation = Quaternion.Slerp(startRotation, rotated, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        targetObject.transform.rotation = rotated;

        if (otherCard != null)
            otherCard.SetActive(true);
    }

    // 단일탭 대기 코루틴
    private IEnumerator SingleTapDelay()
    {
        yield return new WaitForSeconds(doubleTapMaxDelay);
        if (waitingForSecondTap && !isRotating)
        {
            StartCoroutine(RotateAndReturn(targetObject));
        }
        waitingForSecondTap = false;
    }
} 