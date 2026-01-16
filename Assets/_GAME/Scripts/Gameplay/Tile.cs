using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public int ItemType; // Renk ID'si (0: Blue, 1: Green vs.)

    private SpriteRenderer _renderer;
    private TileSkin _currentSkin; // Bu taþýn sahip olduðu renk seti

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Initialize artýk direkt TileSkin alýyor
    public void Initialize(int x, int y, int itemType, TileSkin skin, float targetSize, int sortingOrder)
    {
        this.x = x;
        this.y = y;
        this.ItemType = itemType;
        this._currentSkin = skin; // Skin'i hafýzaya at

        // Baþlangýçta default resmi koy
        _renderer.sprite = skin.defaultSprite;
        _renderer.sortingOrder = sortingOrder;

        // Auto-Fit (Daha önce yazdýðýmýz kod)
        transform.localScale = Vector3.one;
        if (_renderer.sprite != null)
        {
            float spriteWidth = _renderer.bounds.size.x;
            float spriteHeight = _renderer.bounds.size.y;
            float maxDimension = Mathf.Max(spriteWidth, spriteHeight);
            if (maxDimension > 0)
            {
                float newScale = targetSize / maxDimension;
                transform.localScale = Vector3.one * newScale;
            }
        }

        name = $"Tile_{x}_{y}";
    }

    // YENÝ METOD: Dýþarýdan sadece "Durum A olsun" diyoruz, o kendi rengini buluyor
    public void UpdateVisualState(int groupCount)
    {
        if (groupCount >= 9)
        {
            if (_renderer.sprite != _currentSkin.stateCSprite)
                _renderer.sprite = _currentSkin.stateCSprite;
        }
        else if (groupCount >= 7)
        {
            if (_renderer.sprite != _currentSkin.stateBSprite)
                _renderer.sprite = _currentSkin.stateBSprite;
        }
        else if (groupCount >= 5)
        {
            if (_renderer.sprite != _currentSkin.stateASprite)
                _renderer.sprite = _currentSkin.stateASprite;
        }
        else
        {
            // Grup küçükse veya tekse Default haline dön
            if (_renderer.sprite != _currentSkin.defaultSprite)
                _renderer.sprite = _currentSkin.defaultSprite;
        }
    }
}