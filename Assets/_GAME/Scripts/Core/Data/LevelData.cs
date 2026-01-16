using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Board Dimensions (Tahta Boyutu)")]
    [Range(2, 10)] public int rows = 9;
    [Range(2, 10)] public int columns = 9;

    [Header("Game Settings (Oyun Kurallarý)")]
    [Range(1, 6)] public int colorCount = 6;

    // A, B, C Þartlarý
    [Header("Icon Conditions (Ýkon Deðiþim Þartlarý)")]
    public int conditionA = 4;
    public int conditionB = 7;
    public int conditionC = 9;
}