using UnityEngine;
using System.Collections;

public class TapRotateController : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject sparkEffect;
    private bool isRotating = false;
    private ARSessionManager sessionManager;
    private Vector2 tapStartPos;
    private float tapStartTime;
    private const float dragThreshold = 10f;
    private const float timeThreshold = 0.2f;

    private void Start()
    {
        sessionManager = FindObjectOfType<ARSessionManager>();
    }

    private void Update()
    {
        if (isRotating || sessionManager == null || sessionManager.CurrentMotionState != ARSessionManager.MotionState.None)
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
                if (!isRotating)
                    StartCoroutine(RotateAndReturn(targetObject));
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
                    if (!isRotating)
                        StartCoroutine(RotateAndReturn(targetObject));
                }
            }
        }
#endif
    }

    private IEnumerator RotateAndReturn(GameObject targetObject)
    {
        isRotating = true;
        if (sessionManager != null)
            sessionManager.CurrentMotionState = ARSessionManager.MotionState.Tap;
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
        if (sessionManager != null)
            sessionManager.CurrentMotionState = ARSessionManager.MotionState.None;
        if (sparkEffect != null)
            sparkEffect.SetActive(false);
    }
} 