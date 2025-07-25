using UnityEngine;
using System.Collections;

public class DragRotateController : MonoBehaviour
{
    private bool isDragging = false;
    private bool dragMotionActive = false;
    private Vector2 lastTouchPos;
    private Vector2 dragStartPos;
    private float touchStartTime;
    private Coroutine returnCoroutine;
    [SerializeField] public Card card;
    private const float dragThreshold = 10f;
    private const float timeThreshold = 0.2f;
    [SerializeField] private float dragSensitivity = 0.3f;
    [SerializeField] public GameObject targetObject;
    public float xRot = 90f, yRot = 0f, zRot = 0f;

    private void Update()
    {
        if (card == null || card.CurrentMotionState != Card.MotionState.None)
            return;
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = false;
            dragMotionActive = false;
            lastTouchPos = Input.mousePosition;
            dragStartPos = Input.mousePosition;
            touchStartTime = Time.time;
        }
        else if (Input.GetMouseButton(0))
        {
            float dragDist = (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(dragStartPos.x, dragStartPos.y)).magnitude;
            float dragTime = Time.time - touchStartTime;
            if (!dragMotionActive && (dragDist > dragThreshold || dragTime > timeThreshold))
            {
                isDragging = true;
                dragMotionActive = true;
                xRot = 90f; yRot = 90f; zRot = 90f;
                lastTouchPos = dragStartPos;
                if (returnCoroutine != null) { StopCoroutine(returnCoroutine); returnCoroutine = null; }
            }
            if (dragMotionActive)
            {
                float deltaX = Input.mousePosition.x - lastTouchPos.x;
                xRot -= deltaX * dragSensitivity;
                lastTouchPos = Input.mousePosition;
                ApplyRotation();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            float dragDist = (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(dragStartPos.x, dragStartPos.y)).magnitude;
            float dragTime = Time.time - touchStartTime;
            if (dragMotionActive)
            {
                isDragging = false;
                dragMotionActive = false;
                returnCoroutine = StartCoroutine(RotateXToNearest90());
            }
        }
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                isDragging = false;
                dragMotionActive = false;
                lastTouchPos = t.position;
                dragStartPos = t.position;
                touchStartTime = Time.time;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                float dragDist = (t.position - dragStartPos).magnitude;
                float dragTime = Time.time - touchStartTime;
                if (!dragMotionActive && (dragDist > dragThreshold || dragTime > timeThreshold))
                {
                    isDragging = true;
                    dragMotionActive = true;
                    xRot = 90f; yRot = 90f; zRot = 90f;
                    lastTouchPos = dragStartPos;
                    if (returnCoroutine != null) { StopCoroutine(returnCoroutine); returnCoroutine = null; }
                }
                if (dragMotionActive)
                {
                    float deltaX = t.position.x - lastTouchPos.x;
                    xRot -= deltaX * dragSensitivity;
                    lastTouchPos = t.position;
                    ApplyRotation();
                }
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                float dragDist = (t.position - dragStartPos).magnitude;
                float dragTime = Time.time - touchStartTime;
                if (dragMotionActive)
                {
                    isDragging = false;
                    dragMotionActive = false;
                    returnCoroutine = StartCoroutine(RotateXToNearest90());
                }
            }
        }
#endif
    }

    private IEnumerator RotateXToNearest90()
    {
        if (card != null)
            card.CurrentMotionState = Card.MotionState.Drag;
        float startX = xRot;
        float targetX = FindNearest90(xRot);
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(startX, targetX));
        float duration = Mathf.Clamp(angleDiff / 180f, 0.1f, 1f); // 180도=1초, 90도=0.5초, 45도=0.25초, 최소 0.1초, 최대 1초
        float elapsed = 0f;
        float curY = yRot, curZ = zRot;
        while (elapsed < duration)
        {
            if (isDragging)
            {
                if (card != null)
                    card.CurrentMotionState = Card.MotionState.None;
                yield break;
            }
            xRot = Mathf.LerpAngle(startX, targetX, elapsed / duration);
            targetObject.transform.rotation = Quaternion.Euler(xRot, curY, curZ);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // 360으로 나눴을 때 90이 되도록 보정
        xRot = NormalizeTo90(xRot);
        targetObject.transform.rotation = Quaternion.Euler(xRot, curY, curZ);

        xRot = 90f;
        if (card != null)
            card.CurrentMotionState = Card.MotionState.None;
    }

    // x가 90 + 360n이 되도록 보정
    private float NormalizeTo90(float x)
    {
        float normalized = ((x - 90f) % 360f + 360f) % 360f + 90f;
        if (normalized > 450f) normalized -= 360f;
        return normalized;
    }

    private float FindNearest90(float angle)
    {
        // 90 + 360n에 가장 가까운 값
        float n = Mathf.Round((angle - 90f) / 360f);
        return 90f + 360f * n;
    }

    public Coroutine mergeSpinCoroutine;
    public void StartMergeSpin(float speed = 180f)
    {
        xRot = 0f; yRot = 0f; zRot = 0f; // 각도 초기화
        if (mergeSpinCoroutine != null)
            StopCoroutine(mergeSpinCoroutine);
        mergeSpinCoroutine = StartCoroutine(MergeSpinLoop(speed));
    }
    public void StopMergeSpin()
    {
        if (mergeSpinCoroutine != null)
        {
            StopCoroutine(mergeSpinCoroutine);
            mergeSpinCoroutine = null;
        }
    }
    private IEnumerator MergeSpinLoop(float speed)
    {
        float curX = 0f, curZ = 0f; // x, z 각도 고정
        while (true)
        {
            yRot -= speed * Time.deltaTime; // y축 회전
            yRot %= 360f;
            targetObject.transform.rotation = Quaternion.Euler(curX, yRot, curZ);
            yield return null;
        }
    }
    public void ApplyRotation()
    {
        if (targetObject != null)
            targetObject.transform.rotation = Quaternion.Euler(xRot, yRot, zRot);
    }
}