using UnityEngine;
using UnityEngine.Pool; // Unity 2021+ ve Unity 6 için Pool kütüphanesi

// Senin yazdýðýn SceneOnly Singleton'ý kullanýyoruz
public class BoardManager : MonoBehaviourSingletonSceneOnly<BoardManager>
{
    [Header("Configs")]
    public BoardConfig boardConfig;   // Genel Ayarlar
    public LevelData currentLevel;    // O anki Level (M, N, K)

    // Grid Verisi
    public Tile[,] Grid { get; private set; } // Diðer scriptler okuyabilsin diye public property

    // Optimizasyon için Object Pool
    private ObjectPool<Tile> _pool;

    // Singleton Awake'ini ezmemek için override ediyoruz
    public override void Awake()
    {
        base.Awake(); // Singleton kurulumunu yap
        InitializePool();
    }

    private void Start()
    {
        if (currentLevel != null && boardConfig != null)
        {
            GenerateBoard();
        }
        else
        {
            Debug.LogError("LevelData veya BoardConfig eksik!");
        }
    }

    private void InitializePool()
    {
        _pool = new ObjectPool<Tile>(
            createFunc: () =>
            {
                return Instantiate(boardConfig.tilePrefab, transform).GetComponent<Tile>();
            },
            actionOnGet: (tile) => tile.gameObject.SetActive(true),
            actionOnRelease: (tile) => tile.gameObject.SetActive(false),
            actionOnDestroy: (tile) => Destroy(tile.gameObject),
            defaultCapacity: boardConfig.poolDefaultSize,
            maxSize: boardConfig.poolMaxSize
        );
    }

    public void GenerateBoard()
    {
        // Grid array'ini oluþtur
        Grid = new Tile[currentLevel.columns, currentLevel.rows];

        // Grid'i ortalamak için baþlangýç pozisyonu hesabý
        float totalWidth = (currentLevel.columns * boardConfig.tileSize) +
                           ((currentLevel.columns - 1) * boardConfig.padding);
        float totalHeight = (currentLevel.rows * boardConfig.tileSize) +
                            ((currentLevel.rows - 1) * boardConfig.padding);

        Vector2 startPos = new Vector2(
            -totalWidth / 2f + boardConfig.tileSize / 2f,
            -totalHeight / 2f + boardConfig.tileSize / 2f
        );

        // Döngü ile Tile oluþturma
        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                // Rastgele renk seç (K deðeri kadar)
                int randomType = Random.Range(0, currentLevel.colorCount);

                // Havuzdan çek
                Tile newTile = _pool.Get();

                // Pozisyonu ayarla
                float xPos = startPos.x + (x * (boardConfig.tileSize + boardConfig.padding));
                float yPos = startPos.y + (y * (boardConfig.tileSize + boardConfig.padding));

                newTile.transform.position = new Vector2(xPos, yPos);
                newTile.transform.localScale = Vector3.one * boardConfig.tileSize;

                // Görseli ayarla
                Sprite sprite = boardConfig.iconSprites.Length > randomType ?
                                boardConfig.iconSprites[randomType] : null;

                newTile.Initialize(x, y, randomType, sprite, boardConfig.tileSize);

                Grid[x, y] = newTile;
            }
        }

        AdjustCamera(totalWidth, totalHeight);
    }

    private void AdjustCamera(float boardWidth, float boardHeight)
    {
        Camera.main.transform.position = new Vector3(0, 0, -10f);

        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = boardHeight / 2f + 1f;
        float horizontalSize = (boardWidth / 2f + 1f) / aspectRatio;

        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
}