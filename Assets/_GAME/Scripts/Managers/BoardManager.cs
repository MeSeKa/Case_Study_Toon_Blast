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

    public Tile[,] Grid { get; private set; }
    private bool _isProcessing = false;
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
                List<Tile> connectedTiles = MatchFinder.FindMatches(clickedTile, Grid, currentLevel.rows, currentLevel.columns);

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

        float totalWidth = (currentLevel.columns * boardConfig.tileSize) + ((currentLevel.columns - 1) * boardConfig.padding);
        float totalHeight = (currentLevel.rows * boardConfig.tileSize) + ((currentLevel.rows - 1) * boardConfig.padding);
        Vector2 startPos = new Vector2(-totalWidth / 2f + boardConfig.tileSize / 2f, -totalHeight / 2f + boardConfig.tileSize / 2f);

        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                int randomType = Random.Range(0, currentLevel.colorCount);
                Tile newTile = _pool.Get();

                float xPos = startPos.x + (x * (boardConfig.tileSize + boardConfig.padding));
                float yPos = startPos.y + (y * (boardConfig.tileSize + boardConfig.padding));

                newTile.transform.position = new Vector2(xPos, yPos);
                int sortingOrder = (currentLevel.rows + y) * 10;
                Sprite sprite = boardConfig.iconSprites.Length > randomType ? boardConfig.iconSprites[randomType] : null;

                newTile.Initialize(x, y, randomType, sprite, boardConfig.tileSize, sortingOrder);
                Grid[x, y] = newTile;
            }
        }
        AdjustCamera(totalWidth, totalHeight);
    }

    // --- 1. PATLATMA ---
    private IEnumerator ExplodeTiles(List<Tile> matches)
    {
        _isProcessing = true;

        foreach (Tile tile in matches)
        {
            Grid[tile.x, tile.y] = null;
        }

        foreach (Tile tile in matches)
        {
            tile.transform.DOScale(Vector3.zero, boardConfig.explodeDuration)
                .SetEase(boardConfig.explodeEase)
                .OnComplete(() => _pool.Release(tile));
        }

        yield return new WaitForSeconds(boardConfig.explodeDuration);
        StartCoroutine(ApplyGravity());
    }

    // --- 2. YERÇEKÝMÝ ---
    private IEnumerator ApplyGravity()
    {
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
                        Grid[x, writeY] = tile;
                        Grid[x, y] = null;
                        tile.x = x;
                        tile.y = writeY;

                        float targetX = -((currentLevel.columns * boardConfig.tileSize + (currentLevel.columns - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + x * (boardConfig.tileSize + boardConfig.padding);
                        float targetY = -((currentLevel.rows * boardConfig.tileSize + (currentLevel.rows - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + writeY * (boardConfig.tileSize + boardConfig.padding);

                        int newSortingOrder = (currentLevel.rows + writeY) * 10;
                        tile.GetComponent<SpriteRenderer>().sortingOrder = newSortingOrder;

                        tile.transform.DOMove(new Vector2(targetX, targetY), boardConfig.fallDuration)
                            .SetEase(boardConfig.fallEase, boardConfig.gravityOvershoot);
                    }
                    writeY++;
                }
            }
        }

        // --- DEÐÝÞÝKLÝK BURADA: REFILL DELAY ---
        // Eðer config'de bir gecikme varsa bekle, yoksa sadece frame atla.
        if (boardConfig.refillDelay > 0f)
        {
            yield return new WaitForSeconds(boardConfig.refillDelay);
        }
        else
        {
            yield return null;
        }

        StartCoroutine(FillBoard());
    }

    // --- 3. REFILL ---
    private IEnumerator FillBoard()
    {
        float startOffsetY = -((currentLevel.rows * boardConfig.tileSize + (currentLevel.rows - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f;
        float boardTopEdge = startOffsetY + (currentLevel.rows * (boardConfig.tileSize + boardConfig.padding));

        for (int x = 0; x < currentLevel.columns; x++)
        {
            int newTileCountInColumn = 0;

            for (int y = 0; y < currentLevel.rows; y++)
            {
                if (Grid[x, y] == null)
                {
                    int randomType = Random.Range(0, currentLevel.colorCount);
                    Tile newTile = _pool.Get();

                    float targetX = -((currentLevel.columns * boardConfig.tileSize + (currentLevel.columns - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + x * (boardConfig.tileSize + boardConfig.padding);
                    float targetY = startOffsetY + y * (boardConfig.tileSize + boardConfig.padding);

                    float spawnY = boardTopEdge + (newTileCountInColumn * (boardConfig.tileSize + boardConfig.padding));
                    newTile.transform.position = new Vector2(targetX, spawnY);
                    newTileCountInColumn++;

                    int sortingOrder = (currentLevel.rows + y) * 10;
                    Sprite sprite = boardConfig.iconSprites.Length > randomType ? boardConfig.iconSprites[randomType] : null;

                    newTile.Initialize(x, y, randomType, sprite, boardConfig.tileSize, sortingOrder);
                    Grid[x, y] = newTile;

                    newTile.transform.DOMove(new Vector2(targetX, targetY), boardConfig.refillDuration)
                        .SetEase(boardConfig.refillEase, boardConfig.refillOvershoot);
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