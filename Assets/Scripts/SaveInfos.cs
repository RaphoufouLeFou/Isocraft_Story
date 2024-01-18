using UnityEngine;

public static class SaveInfos
{
    public static string SaveName { get; set; }
    public static Vector3 PlayerPosition { get; set; }
    public static Vector3 PlayerRotation { get; set; }
    public static Inventory PlayerInventory { get; set; }
    public static bool HasBeenLoaded { get; set; }
}
