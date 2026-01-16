using UnityEngine;
using DG.Tweening; // Ease enum'ý için gerekli

[CreateAssetMenu(fileName = "BoardConfig", menuName = "Game/Board Config")]
public class BoardConfig : ScriptableObject
{
    [Header("Visual Layout")]
    public float tileSize = 1.0f;
    public float padding = 0.1f;

    [Header("Assets")]
    public GameObject tilePrefab;
    public Sprite[] iconSprites;

    [Header("Optimization")]
    public int poolDefaultSize = 100;
    public int poolMaxSize = 300;

    // --- YENÝ EKLENEN KISIM: ANIMATION SETTINGS ---
    [Header("Animation Settings")]

    [Tooltip("Patlama animasyonu süresi")]
    public float explodeDuration = 0.2f;
    public Ease explodeEase = Ease.InBack;

    [Tooltip("Taþlarýn aþaðý kayma süresi")]
    public float fallDuration = 0.4f;
    public Ease fallEase = Ease.OutBounce;

    [Tooltip("Yeni taþlarýn yukarýdan gelme süresi")]
    public float refillDuration = 0.5f;
    public Ease refillEase = Ease.OutBack;
}