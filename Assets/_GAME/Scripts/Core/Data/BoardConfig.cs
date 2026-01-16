using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[CreateAssetMenu(fileName = "BoardConfig", menuName = "Game/Board Config")]
public class BoardConfig : ScriptableObject
{
    [Header("Layout Settings")]
    public float tileSize = 1.0f;
    public float padding = 0.1f;
    [Tooltip("Extra margin for the camera view")]
    public float cameraMargin = 1.0f;

    [Header("Assets")]
    public GameObject tilePrefab;
    public List<TileSkin> tileSkins;

    [Header("Pooling")]
    public int poolDefaultSize = 100;
    public int poolMaxSize = 300;

    [Header("Animation: Explosion")]
    public float explodeDuration = 0.2f;
    public Ease explodeEase = Ease.InBack;

    [Header("Animation: Gravity")]
    public float fallDuration = 0.4f;
    public Ease fallEase = Ease.OutBack;
    [Range(0f, 2f)] public float gravityOvershoot = 0.6f;

    [Header("Animation: Refill")]
    public float refillDelay = 0.2f;
    public float refillDuration = 0.4f;
    public Ease refillEase = Ease.OutBack;
    [Range(0f, 2f)] public float refillOvershoot = 0.85f;

    [Header("Animation: Shuffle & Logic")]
    public float shuffleStepDelay = 0.2f;
    public float shuffleMoveDuration = 0.5f;
    public Ease shuffleEase = Ease.InOutQuad;

    public float injectionDuration = 0.4f;

    public float swapDuration = 0.5f;
    public Ease swapEase = Ease.OutBack;

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