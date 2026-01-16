using UnityEngine;
using DG.Tweening;

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

    [Header("Explode Settings")]
    public float explodeDuration = 0.2f;
    public Ease explodeEase = Ease.InBack;

    [Header("Gravity Settings")]
    public float fallDuration = 0.4f;
    public Ease fallEase = Ease.OutBack;
    [Range(0f, 2f)] public float gravityOvershoot = 0.6f;

    [Header("Refill Settings")]
    [Tooltip("Yeni taþlar gelmeden önce ne kadar beklensin? (0 = Bekleme yok)")]
    public float refillDelay = 0.2f; // Varsayýlan bir gecikme ekledik

    public float refillDuration = 0.4f;
    public Ease refillEase = Ease.OutBack;
    [Range(0f, 2f)] public float refillOvershoot = 0.85f;
}