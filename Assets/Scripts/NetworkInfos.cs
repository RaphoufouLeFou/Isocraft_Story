using UnityEngine;

public static class NetworkInfos
{
    public static bool IsMultiplayerGame { get; set; }
    public static bool IsHost { get; set; }
    public static bool StartedFromMainMenu = false;
    public static Vector3 PlayerPos { get; set; }
}
