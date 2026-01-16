using UnityEngine;
using UnityEngine.UI; // UI kullanacaksan (SpriteRenderer ise burayý sil)

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    
    // Bloðun hangi tip (renk) olduðunu tutar (0: Mavi, 1: Kýrmýzý vb.)
    public int ItemType; 

    // Görseli deðiþtirmek için SpriteRenderer referansý
    // Eðer UI (Canvas) üzerinde çalýþýyorsan 'Image', World Space ise 'SpriteRenderer' kullan.
    // Þimdilik World Space (SpriteRenderer) varsayýyorum.
    [SerializeField] private SpriteRenderer _renderer;

    // Týklandýðýnda BoardManager'a haber vermek için bir fonksiyon
    // Bunu ileride Input sistemine baðlayacaðýz.
    // Assets/_Game/Scripts/Gameplay/Tile.cs

    public void Initialize(int x, int y, int itemType, Sprite sprite, float targetSize, int sortingOrder) // Yeni parametre
    {
        this.x = x;
        this.y = y;
        this.ItemType = itemType;

        if (sprite != null)
        {
            _renderer.sprite = sprite;
            _renderer.sortingOrder = sortingOrder;

            // --- AUTO FIT ---
            transform.localScale = Vector3.one;
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
}