using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[CreateAssetMenu(fileName = "BoardConfig", menuName = "Game/Board Config")]
public class BoardConfig : ScriptableObject
{
    [Header("Visual Layout")]
    public float tileSize = 1.0f;
    public float padding = 0.1f;

    [Header("Assets")]
    public GameObject tilePrefab;
    public List<TileSkin> tileSkins;

    [Header("Optimization")]
    public int poolDefaultSize = 100;
    public int poolMaxSize = 300;

    [Header("Explode Settings")]
    public float explodeDuration = 0.2f;
    public Ease explodeEase = Ease.InBack;

    [Header("Gravity Settings")]
    public float fallDuration = 0.4f;
    public Ease fallEase = Ease.OutBack;
    [Range(0f, 2f)] public float gravityOvershoot = 0.6f;

    [Header("Refill Settings")]
    public float refillDelay = 0.2f;
    public float refillDuration = 0.4f;
    public Ease refillEase = Ease.OutBack;
    [Range(0f, 2f)] public float refillOvershoot = 0.85f;
}

// Bu struct sayesinde Inspector'da her rengi ayrý ayrý paketleyebileceksin
[System.Serializable]
public struct TileSkin
{
    public string name; // Örn: "Blue" (Karýþmasýn diye)
    public Sprite defaultSprite; // Blue_Default
    public Sprite stateASprite;  // Blue_A (5-7 arasý)
    public Sprite stateBSprite;  // Blue_B (7-9 arasý)
    public Sprite stateCSprite;  // Blue_C (9+ üstü)
}