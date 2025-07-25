using UnityEngine;

public class Card : MonoBehaviour
{
    public enum MotionState { None, Tap, Drag, DoubleTap }
    public MotionState CurrentMotionState = MotionState.None;

    public enum CardType { Red, Blue }
    public CardType Type;
} 