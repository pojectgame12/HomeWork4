/// <summary>Touch button input shared by player and lamp.</summary>
public static class MobileInputState
{
    public static bool left, right, rotateLeft, rotateRight;
    private static bool jumpRequested, lampRequested;
    public static void RequestJump() { jumpRequested = true; }
    public static void RequestLamp() { lampRequested = true; }
    public static bool ConsumeJump() { bool v = jumpRequested; jumpRequested = false; return v; }
    public static bool ConsumeLamp() { bool v = lampRequested; lampRequested = false; return v; }
    public static void Clear()
    {
        left = right = rotateLeft = rotateRight = jumpRequested = lampRequested = false;
    }
}
