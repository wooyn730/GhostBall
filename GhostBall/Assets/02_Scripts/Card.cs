using UnityEngine;

public class Card : MonoBehaviour
{
    public enum MotionState { None, Tap, Drag, DoubleTap }
    public MotionState CurrentMotionState = MotionState.None;

    public enum CardType { Red, Blue }
    public CardType Type;

    // 카드별 추가 데이터 및 기능 필요시 여기에 작성
} 