using UnityEngine;
using UnityEngine.EventSystems;

public sealed class TouchControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Action { Left, Right, RotateLeft, RotateRight, Jump, Lamp }
    public Action action;

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (action)
        {
            case Action.Left: MobileInputState.left = true; break;
            case Action.Right: MobileInputState.right = true; break;
            case Action.RotateLeft: MobileInputState.rotateLeft = true; break;
            case Action.RotateRight: MobileInputState.rotateRight = true; break;
            case Action.Jump: MobileInputState.RequestJump(); break;
            case Action.Lamp: MobileInputState.RequestLamp(); break;
        }
    }
    public void OnPointerUp(PointerEventData eventData) { Release(); }
    public void OnPointerExit(PointerEventData eventData) { Release(); }
    private void OnDisable() { Release(); }
    private void Release()
    {
        switch (action)
        {
            case Action.Left: MobileInputState.left = false; break;
            case Action.Right: MobileInputState.right = false; break;
            case Action.RotateLeft: MobileInputState.rotateLeft = false; break;
            case Action.RotateRight: MobileInputState.rotateRight = false; break;
        }
    }
}
