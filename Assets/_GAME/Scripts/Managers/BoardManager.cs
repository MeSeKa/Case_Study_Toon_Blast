using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.InputSystem;

public class BoardManager : MonoBehaviourSingletonSceneOnly<BoardManager>
{
    [Header("Configuration")]
    public BoardConfig boardConfig;
    public LevelData currentLevel;

    // Grid Verisi
    public Tile[,] Grid { get; private set; }

    // Ýþlem kontrolü
    private bool _isProcessing = false;

    // Optimizasyon için Object Pool
    private ObjectPool<Tile> _pool;

    public override void Awake()
    {
        base.Awake();
        InitializePool();
    }

    private void Start()
    {
        if (currentLevel != null && boardConfig != null)
        {
            GenerateBoard();
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

    private void Update()
    {
        if (_isProcessing) return;

        // Pointer: Hem Mouse sol týkýný hem de Mobil dokunmasýný algýlar.
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        Vector2 pointerPos = Pointer.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(pointerPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Tile clickedTile = hit.collider.GetComponent<Tile>();
            if (clickedTile != null)
            {
                // MatchFinder ile hesaplama
                List<Tile> connectedTiles = MatchFinder.FindMatches(
                    clickedTile,
                    Grid,
                    currentLevel.rows,
                    currentLevel.columns
                );

                if (connectedTiles.Count >= 2)
                {
                    StartCoroutine(ExplodeTiles(connectedTiles));
                }
            }
        }
    }

    private void GenerateBoard()
    {
        Grid = new Tile[currentLevel.columns, currentLevel.rows];

        float totalWidth = (currentLevel.columns * boardConfig.tileSize) +
                           ((currentLevel.columns - 1) * boardConfig.padding);
        float totalHeight = (currentLevel.rows * boardConfig.tileSize) +
                            ((currentLevel.rows - 1) * boardConfig.padding);

        Vector2 startPos = new Vector2(
            -totalWidth / 2f + boardConfig.tileSize / 2f,
            -totalHeight / 2f + boardConfig.tileSize / 2f
        );

        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                int randomType = Random.Range(0, currentLevel.colorCount);

                Tile newTile = _pool.Get();

                float xPos = startPos.x + (x * (boardConfig.tileSize + boardConfig.padding));
                float yPos = startPos.y + (y * (boardConfig.tileSize + boardConfig.padding));

                newTile.transform.position = new Vector2(xPos, yPos);

                // Sorting Order mantýðý (Senin formülün)
                int sortingOrder = (currentLevel.rows + y) * 10;

                Sprite sprite = boardConfig.iconSprites.Length > randomType ?
                                boardConfig.iconSprites[randomType] : null;

                newTile.Initialize(x, y, randomType, sprite, boardConfig.tileSize, sortingOrder);

                Grid[x, y] = newTile;
            }
        }

        AdjustCamera(totalWidth, totalHeight);
    }

    private IEnumerator ExplodeTiles(List<Tile> matches)
    {
        _isProcessing = true;

        // 1. Grid verisini temizle
        foreach (Tile tile in matches)
        {
            Grid[tile.x, tile.y] = null;
        }

        // 2. Görsel temizlik (Animasyon)
        foreach (Tile tile in matches)
        {
            // Config'den gelen süre ve ease kullanýlýyor
            tile.transform.DOScale(Vector3.zero, boardConfig.explodeDuration)
                .SetEase(boardConfig.explodeEase)
                .OnComplete(() => _pool.Release(tile));
        }

        // Animasyon süresi kadar bekle
        yield return new WaitForSeconds(boardConfig.explodeDuration);

        // Patlama bitti, yerçekimini baþlat
        StartCoroutine(ApplyGravity());
    }

    // --- GRAVITY: MEVCUT TAÞLARIN DÜÞMESÝ ---
    private IEnumerator ApplyGravity()
    {
        float maxDuration = 0f; // En uzun animasyonu takip etmek için

        for (int x = 0; x < currentLevel.columns; x++)
        {
            int writeY = 0;

            for (int y = 0; y < currentLevel.rows; y++)
            {
                Tile tile = Grid[x, y];

                if (tile != null)
                {
                    if (y != writeY)
                    {
                        // 1. Mantýksal Taþýma
                        Grid[x, writeY] = tile;
                        Grid[x, y] = null;

                        tile.x = x;
                        tile.y = writeY;

                        // 2. Görsel Taþýma (Pozisyon Hesabý)
                        float targetX = -((currentLevel.columns * boardConfig.tileSize + (currentLevel.columns - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + x * (boardConfig.tileSize + boardConfig.padding);
                        float targetY = -((currentLevel.rows * boardConfig.tileSize + (currentLevel.rows - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + writeY * (boardConfig.tileSize + boardConfig.padding);
                        Vector2 targetPos = new Vector2(targetX, targetY);

                        // Sorting Order Güncelle
                        int newSortingOrder = (currentLevel.rows + writeY) * 10;
                        tile.GetComponent<SpriteRenderer>().sortingOrder = newSortingOrder;

                        // Config'den gelen süre
                        float currentMoveDuration = boardConfig.fallDuration;

                        // En uzun süreyi güncelle
                        if (currentMoveDuration > maxDuration)
                        {
                            maxDuration = currentMoveDuration;
                        }

                        // Animasyonu baþlat
                        tile.transform.DOMove(targetPos, currentMoveDuration)
                            .SetEase(boardConfig.fallEase);
                    }
                    writeY++;
                }
            }
        }

        // En uzun animasyon bitene kadar bekle (+ ufak bir pay)
        yield return new WaitForSeconds(maxDuration + 0.05f);

        StartCoroutine(FillBoard());
    }

    // --- REFILL: YENÝ TAÞLARIN OLUÞMASI ---
    private IEnumerator FillBoard()
    {
        // 1. Tavanýn (Board'ýn bittiði yerin) Y koordinatýný bulalým
        float startOffsetY = -((currentLevel.rows * boardConfig.tileSize + (currentLevel.rows - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f;

        // Bu, en üst satýrýn (görünür alanýn) bittiði yerin hemen üstü
        float boardTopEdge = startOffsetY + (currentLevel.rows * (boardConfig.tileSize + boardConfig.padding));

        for (int x = 0; x < currentLevel.columns; x++)
        {
            // BU SÜTUNDA KAÇ TANE YENÝ TAÞ ÜRETTÝK?
            // Bu sayaç sayesinde taþlarý üst üste dizeceðiz.
            int newTileCountInColumn = 0;

            for (int y = 0; y < currentLevel.rows; y++)
            {
                // Gravity sonrasý boþluklar her zaman en üstte toplanmýþtýr.
                // Yani y=0 doludur, y=5,6,7 boþtur gibi...
                if (Grid[x, y] == null)
                {
                    int randomType = Random.Range(0, currentLevel.colorCount);
                    Tile newTile = _pool.Get();

                    // HEDEF Pozisyon (Gideceði yer)
                    float targetX = -((currentLevel.columns * boardConfig.tileSize + (currentLevel.columns - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + x * (boardConfig.tileSize + boardConfig.padding);
                    float targetY = startOffsetY + y * (boardConfig.tileSize + boardConfig.padding);

                    // --- BAÞLANGIÇ POZÝSYONU (KRÝTÝK KISIM) ---
                    // Mantýk: Tahtanýn tepesi + (Bu sütunda kaçýncý taþ * Taþ Boyutu)
                    // Örn: Ýlk taþ hemen sýnýrdan baþlar. Ýkinci taþ onun üstünden baþlar.
                    float spawnY = boardTopEdge + (newTileCountInColumn * (boardConfig.tileSize + boardConfig.padding));

                    newTile.transform.position = new Vector2(targetX, spawnY);

                    // Sýradaki taþ bunun da üstüne gelsin diye sayacý artýrýyoruz
                    newTileCountInColumn++;

                    int sortingOrder = (currentLevel.rows + y) * 10;
                    Sprite sprite = boardConfig.iconSprites.Length > randomType ?
                                    boardConfig.iconSprites[randomType] : null;

                    newTile.Initialize(x, y, randomType, sprite, boardConfig.tileSize, sortingOrder);

                    Grid[x, y] = newTile;

                    // Düþme Animasyonu
                    // Sequence kullanarak hafif bir gecikme (Stagger) eklersen daha doðal durur
                    // Ama þimdilik senin istediðin o "üst üste binmeme" olayýný sadece pozisyonla çözdük.
                    newTile.transform.DOMove(new Vector2(targetX, targetY), boardConfig.refillDuration)
                        .SetEase(boardConfig.refillEase);
                }
            }
        }

        yield return new WaitForSeconds(boardConfig.refillDuration);

        _isProcessing = false;
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