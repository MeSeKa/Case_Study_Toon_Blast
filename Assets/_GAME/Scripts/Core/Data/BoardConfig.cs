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

    [Header("Shuffle & Animation Timings")]
    [Tooltip("Her shuffle adýmý arasýndaki bekleme süresi")]
    public float shuffleStepDelay = 0.2f;

    [Tooltip("Karýþtýrma sýrasýnda taþlarýn yer deðiþtirme süresi")]
    public float shuffleMoveDuration = 0.5f;

    [Tooltip("Renk enjeksiyonu animasyon süresi")]
    public float injectionDuration = 0.4f;

    [Tooltip("Force Match swap iþlemi süresi")]
    public float swapDuration = 0.5f;

    [Tooltip("Loop döngülerinde sonsuz döngüye girmemek için güvenlik sýnýrý")]
    public int maxCalculationAttempts = 50;
}

[System.Serializable]
public struct TileSkin
{
    public string name;
    public Sprite defaultSprite;
    public Sprite stateASprite;
    public Sprite stateBSprite;
    public Sprite stateCSprite;
}