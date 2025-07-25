using easyar;
using System;
using UnityEngine;

public class ARSessionManager : MonoBehaviour
{
    [SerializeField] private ARSession Session;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Card redCard;
    [SerializeField] private Card blueCard;
    [SerializeField] private GameObject popUp;

    private string deviceModel = string.Empty;
    private ImageTrackerFrameFilter tracker;
    private static Optional<DateTime> trialCounter;
    private bool popUpShown = false;

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void ImportSampleStreamingAssets()
    {
        FileUtil.ImportSampleStreamingAssets();
    }
#endif

    private void Awake()
    {
        SetAllControllersEnabled(false);
        popUp.SetActive(false);
        
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                using (var buildClass = new AndroidJavaClass("android.os.Build"))
                {
                    deviceModel = $"(Device = {buildClass.GetStatic<string>("DEVICE")}, Model = {buildClass.GetStatic<string>("MODEL")})";
                }
            }
            catch (Exception e) { deviceModel = e.Message; }
        }
#endif
        tracker = Session.GetComponentInChildren<ImageTrackerFrameFilter>();

        Session.StateChanged += (state) =>
        {
            if (state == ARSession.SessionState.Ready)
            {
                if (trialCounter.OnNone)
                {
                    trialCounter = DateTime.Now;
                }
            }
        };
    }

    private void Update()
    {
        // 카드 합체 조건 체크
        if (redCard != null && blueCard != null)
        {
            if (redCard.CurrentMotionState == Card.MotionState.DoubleTap && blueCard.gameObject.activeSelf)
            {
                // 각 카드의 TapRotateController의 이펙트/오브젝트 비활성화
                var redTap = redCard.GetComponentInChildren<TapRotateController>();
                if (redTap != null) redTap.DeactivateAllEffects();
                var blueTap = blueCard.GetComponentInChildren<TapRotateController>();
                if (blueTap != null) blueTap.DeactivateAllEffects();
                // (추가) 합체 이펙트 등 원하는 동작
            }
        }

        if (!popUpShown && popUp != null)
        {
            if ((redCard != null && redCard.gameObject.activeSelf) ||
                (blueCard != null && blueCard.gameObject.activeSelf))
            {
                popUp.SetActive(true);
                popUpShown = true;
            }
        }
    }
    
    public void SwitchMotionFusion(bool on)
    {
        tracker.ResultType = new ImageTrackerFrameFilter.ResultParameters { EnablePersistentTargetInstance = on, EnableMotionFusion = on };
    }

    // TapRotateController, DragRotateController를 모두 활성화하는 함수
    public void SetAllControllersEnabled(bool enable)
    {
        if (redCard != null)
        {
            var tap = redCard.GetComponentInChildren<TapRotateController>(true);
            if (tap != null) tap.enabled = enable;
            var drag = redCard.GetComponentInChildren<DragRotateController>(true);
            if (drag != null) drag.enabled = enable;
        }
        if (blueCard != null)
        {
            var tap = blueCard.GetComponentInChildren<TapRotateController>(true);
            if (tap != null) tap.enabled = enable;
            var drag = blueCard.GetComponentInChildren<DragRotateController>(true);
            if (drag != null) drag.enabled = enable;
        }
    }
}