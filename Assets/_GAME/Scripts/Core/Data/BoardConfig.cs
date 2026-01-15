using UnityEngine;

// Tüm oyun genelinde geçerli görsel ve teknik ayarlar
[CreateAssetMenu(fileName = "BoardConfig", menuName = "Game/Board Config")]
public class BoardConfig : ScriptableObject
{
    [Header("Visual Layout")]
    public float tileSize = 1.0f;       // Kare boyutu
    public float padding = -0.1f;        // Kareler arasý boþluk

    [Header("Assets")]
    public GameObject tilePrefab;       // Tile Prefab'ý
    public Sprite[] iconSprites;        // Renklerin Spritelarý (Mavi, Kýrmýzý vb.)

    [Header("Optimization (Pooling)")]
    public int poolDefaultSize = 100;   // Baþlangýç havuz boyutu 10x10 diye düþündüm daha büyüðüne gerek olmayacak genelde
    public int poolMaxSize = 300;       // Maksimum havuz boyutu
}