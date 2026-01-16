using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Board Dimensions")]
    // Vaka çalýþmasýndaki Example 1 (N=12) senaryosunu test edebilmek için 
    // sütun üst sýnýrýný PDF'te belirtilen 10 yerine 15'e çektik.
    [Range(2, 15)] public int rows = 9;
    [Range(2, 15)] public int columns = 9;

    [Header("Game Settings")]
    [Range(1, 6)] public int colorCount = 6;

    [Header("Icon Conditions")]
    // Gruplarýn büyüklüðüne göre hangi ikonun (A, B veya C) gösterileceðini belirleyen eþik deðerler.
    // Örneðin conditionA = 4 ise, 5 ve üzeri sayýda blok içeren gruplar 1. ikonu alýr.
    public int conditionA = 4;
    public int conditionB = 7;
    public int conditionC = 9;
}