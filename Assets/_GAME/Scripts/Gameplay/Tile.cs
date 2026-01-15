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

    public void Initialize(int x, int y, int itemType, Sprite sprite, float targetSize) // targetSize parametresi eklendi
    {
        this.x = x;
        this.y = y;
        this.ItemType = itemType;

        if (sprite != null)
        {
            _renderer.sprite = sprite;

            // --- AUTO FIT (OTOMATÝK SIÐDIRMA) ---
            // 1. Önce scale'i sýfýrla ki orijinal boyutu ölçebilelim
            transform.localScale = Vector3.one;

            // 2. Resmin þu anki dünya boyutunu (Renderer Bounds) al
            // Eðer sprite null ise hata vermesin diye kontrol ediyoruz
            if (_renderer.sprite != null)
            {
                float spriteWidth = _renderer.bounds.size.x;
                float spriteHeight = _renderer.bounds.size.y;

                // 3. Hangisi büyükse (en veya boy) onu baz alarak küçültme oranýný bul
                float maxDimension = Mathf.Max(spriteWidth, spriteHeight);

                // Eðer resim zaten istediðimiz boyuttaysa iþlem yapma (0'a bölme hatasý olmasýn)
                if (maxDimension > 0)
                {
                    float newScale = targetSize / maxDimension;
                    transform.localScale = Vector3.one * newScale;
                }
            }
        }

        name = $"Tile_{x}_{y}";
    }
}