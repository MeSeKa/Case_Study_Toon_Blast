using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public int ItemType;

    private SpriteRenderer _renderer;
    private TileSkin _currentSkin;

    // Cache the correct scale to maintain consistency during animations/pooling
    private Vector3 _correctScale = Vector3.one;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Sets up the tile's data, visual appearance, and sorting order
    public void Initialize(int x, int y, int itemType, TileSkin skin, float targetSize, int sortingOrder)
    {
        this.x = x;
        this.y = y;
        this.ItemType = itemType;
        this._currentSkin = skin;

        _renderer.sprite = skin.defaultSprite;
        _renderer.sortingOrder = sortingOrder;

        // Reset scale before calculating bounds to ensure accuracy
        transform.localScale = Vector3.one;

        if (_renderer.sprite != null)
        {
            float spriteWidth = _renderer.bounds.size.x;
            float spriteHeight = _renderer.bounds.size.y;
            float maxDimension = Mathf.Max(spriteWidth, spriteHeight);

            if (maxDimension > 0)
            {
                float newScaleVal = targetSize / maxDimension;
                _correctScale = Vector3.one * newScaleVal;
            }
        }

        // Apply the calculated scale
        transform.localScale = _correctScale;

        name = $"Tile_{x}_{y}";
    }

    // Updates the visual icon (A, B, C) based on the group size
    public void SetVisualState(int stateIndex)
    {
        Sprite targetSprite = _currentSkin.defaultSprite;

        switch (stateIndex)
        {
            case 3: targetSprite = _currentSkin.stateCSprite; break;
            case 2: targetSprite = _currentSkin.stateBSprite; break;
            case 1: targetSprite = _currentSkin.stateASprite; break;
        }

        if (_renderer.sprite != targetSprite)
        {
            _renderer.sprite = targetSprite;
        }
    }

    // Animates a color change (used for deadlock injection)
    public void AnimateColorChange(int newType, TileSkin newSkin, float duration)
    {
        this.ItemType = newType;
        this._currentSkin = newSkin;

        // Scale down, swap sprite, then scale back up to the correct size
        transform.DOScale(Vector3.zero, duration / 2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            _renderer.sprite = newSkin.defaultSprite;
            transform.DOScale(_correctScale, duration / 2f).SetEase(Ease.OutBack);
        });
    }

    // General move animation (Gravity, Shuffle, Swap)
    public void AnimateMove(Vector3 targetPos, float duration, Ease ease, float overshoot = 0)
    {
        if (overshoot > 0)
            transform.DOMove(targetPos, duration).SetEase(ease, overshoot);
        else
            transform.DOMove(targetPos, duration).SetEase(ease);
    }

    // Spawn/Drop animation for new tiles
    public void AnimateSpawn(Vector3 targetPos, float duration, Ease ease, float overshoot)
    {
        transform.DOMove(targetPos, duration).SetEase(ease, overshoot);
    }

    // Explosion animation when a group is matched
    public void AnimateExplosion(float duration, Ease ease, System.Action onComplete)
    {
        transform.DOScale(Vector3.zero, duration)
            .SetEase(ease)
            .OnComplete(() => onComplete?.Invoke());
    }

    // Instant type change (no animation), mostly for internal logic
    public void ChangeType(int newType, TileSkin newSkin)
    {
        this.ItemType = newType;
        this._currentSkin = newSkin;
        _renderer.sprite = newSkin.defaultSprite;
        transform.localScale = _correctScale; // Ensure correct scale
    }
}