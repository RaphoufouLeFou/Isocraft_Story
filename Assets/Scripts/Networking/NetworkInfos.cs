using System;

public static class NetworkInfos
{
    public static bool IsMultiplayerGame { get; set; }
    public static bool IsHost { get; set; }
    public static bool StartedFromMainMenu = false;
    public static Uri uri { get; set; }
    public static bool IsLocalHost { get; set; }

}
