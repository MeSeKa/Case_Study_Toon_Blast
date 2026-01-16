using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public int ItemType;

    private SpriteRenderer _renderer;
    private TileSkin _currentSkin;

    // YENÝ: Hesaplanan doðru boyutu burada saklayacaðýz
    private Vector3 _correctScale = Vector3.one;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int x, int y, int itemType, TileSkin skin, float targetSize, int sortingOrder)
    {
        this.x = x;
        this.y = y;
        this.ItemType = itemType;
        this._currentSkin = skin;

        _renderer.sprite = skin.defaultSprite;
        _renderer.sortingOrder = sortingOrder;

        // --- SCALE HESABI VE SAKLAMA ---
        transform.localScale = Vector3.one; // Önce sýfýrla ki bounds doðru ölçülsün

        if (_renderer.sprite != null)
        {
            float spriteWidth = _renderer.bounds.size.x;
            float spriteHeight = _renderer.bounds.size.y;
            float maxDimension = Mathf.Max(spriteWidth, spriteHeight);

            if (maxDimension > 0)
            {
                float newScaleVal = targetSize / maxDimension;
                _correctScale = Vector3.one * newScaleVal; // Deðeri sakla!
            }
        }

        // Saklanan deðeri uygula
        transform.localScale = _correctScale;

        name = $"Tile_{x}_{y}";
    }

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
            // Eðer A,B,C ikonlarýnýn boyutlarý defaulttan çok farklýysa 
            // burada tekrar scale hesabý yapýlabilir ama genelde gerekmez.
        }
    }

    public void AnimateColorChange(int newType, TileSkin newSkin, float duration)
    {
        this.ItemType = newType;
        this._currentSkin = newSkin;

        // DÜZELTME: Vector3.one yerine _correctScale kullanýyoruz.
        transform.DOScale(Vector3.zero, duration / 2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            _renderer.sprite = newSkin.defaultSprite;
            // Burada eski haline (hesaplanan doðru boyuta) dönüyor
            transform.DOScale(_correctScale, duration / 2f).SetEase(Ease.OutBack);
        });
    }

    // Force Swap gibi anlýk deðiþimler için
    public void ChangeType(int newType, TileSkin newSkin)
    {
        this.ItemType = newType;
        this._currentSkin = newSkin;
        _renderer.sprite = newSkin.defaultSprite;
        transform.localScale = _correctScale; // Boyutu garantiye al
    }
}