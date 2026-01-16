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
    public bool IsProcessing { get { return _isProcessing; } }

    private ObjectPool<Tile> _pool;

    public override void Awake()
    {
        // Set target frame rate for mobile consistency
        Application.targetFrameRate = 60;
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

    // Initialize object pool for tiles to optimize memory usage
    private void InitializePool()
    {
        _pool = new ObjectPool<Tile>(
            createFunc: () => Instantiate(boardConfig.tilePrefab, transform).GetComponent<Tile>(),
            actionOnGet: (tile) => tile.gameObject.SetActive(true),
            actionOnRelease: (tile) => tile.gameObject.SetActive(false),
            actionOnDestroy: (tile) => Destroy(tile.gameObject),
            defaultCapacity: boardConfig.poolDefaultSize,
            maxSize: boardConfig.poolMaxSize
        );
    }

    private void GenerateBoard()
    {
        Grid = new Tile[currentLevel.columns, currentLevel.rows];

        float totalWidth = (currentLevel.columns * boardConfig.tileSize) + ((currentLevel.columns - 1) * boardConfig.padding);
        float totalHeight = (currentLevel.rows * boardConfig.tileSize) + ((currentLevel.rows - 1) * boardConfig.padding);

        // Center the board
        Vector2 startPos = new Vector2(-totalWidth / 2f + boardConfig.tileSize / 2f, -totalHeight / 2f + boardConfig.tileSize / 2f);

        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                int randomType = Random.Range(0, boardConfig.tileSkins.Count);
                TileSkin selectedSkin = boardConfig.tileSkins[randomType];

                Tile newTile = _pool.Get();
                float xPos = startPos.x + (x * (boardConfig.tileSize + boardConfig.padding));
                float yPos = startPos.y + (y * (boardConfig.tileSize + boardConfig.padding));

                newTile.transform.position = new Vector2(xPos, yPos);
                int sortingOrder = (currentLevel.rows + y) * 10;

                newTile.Initialize(x, y, randomType, selectedSkin, boardConfig.tileSize, sortingOrder);
                Grid[x, y] = newTile;
            }
        }

        AdjustCamera(totalWidth, totalHeight);

        // Update visuals and check for initial deadlocks
        var ctx = new BoardContext(Grid, null, currentLevel.columns, currentLevel.rows);
        BoardVisualizer.UpdateAllIcons(ctx, currentLevel);

        if (DeadlockSolver.IsDeadlocked(ctx))
        {
            _isProcessing = true;
            StartCoroutine(ShuffleBoard());
        }
    }

    public void OnTileClicked(Tile clickedTile)
    {
        if (_isProcessing) return;

        var ctx = new BoardContext(Grid, null, currentLevel.columns, currentLevel.rows);
        List<Tile> connectedTiles = MatchFinder.FindMatches(clickedTile, ctx);

        if (connectedTiles.Count >= 2)
        {
            StartCoroutine(ExplodeTiles(connectedTiles));
        }
    }

    // Handles tile explosion animation and logic
    private IEnumerator ExplodeTiles(List<Tile> matches)
    {
        _isProcessing = true;
        foreach (Tile tile in matches) Grid[tile.x, tile.y] = null;

        foreach (Tile tile in matches)
        {
            tile.AnimateExplosion(
                boardConfig.explodeDuration,
                boardConfig.explodeEase,
                onComplete: () => _pool.Release(tile)
            );
        }

        yield return new WaitForSeconds(boardConfig.explodeDuration);
        StartCoroutine(ApplyGravity());
    }

    // Handles falling tiles (gravity)
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
                        tile.x = x; tile.y = writeY;

                        Vector3 targetPos = GetWorldPosition(x, writeY);
                        int newSortingOrder = (currentLevel.rows + writeY) * 10;
                        tile.GetComponent<SpriteRenderer>().sortingOrder = newSortingOrder;

                        tile.AnimateMove(
                            targetPos,
                            boardConfig.fallDuration,
                            boardConfig.fallEase,
                            boardConfig.gravityOvershoot
                        );
                    }
                    writeY++;
                }
            }
        }

        float delay = boardConfig.refillDelay > 0f ? boardConfig.refillDelay : 0f;
        if (delay > 0) yield return new WaitForSeconds(delay);
        else yield return null;

        StartCoroutine(FillBoard());
    }

    // Refills the board with new tiles falling from the top
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
                    int randomType = Random.Range(0, boardConfig.tileSkins.Count);
                    TileSkin selectedSkin = boardConfig.tileSkins[randomType];

                    Tile newTile = _pool.Get();
                    Vector3 targetPos = GetWorldPosition(x, y);

                    float spawnY = boardTopEdge + (newTileCountInColumn * (boardConfig.tileSize + boardConfig.padding));
                    newTile.transform.position = new Vector2(targetPos.x, spawnY);
                    newTileCountInColumn++;

                    int sortingOrder = (currentLevel.rows + y) * 10;
                    newTile.Initialize(x, y, randomType, selectedSkin, boardConfig.tileSize, sortingOrder);
                    Grid[x, y] = newTile;

                    newTile.AnimateSpawn(
                        targetPos,
                        boardConfig.refillDuration,
                        boardConfig.refillEase,
                        boardConfig.refillOvershoot
                    );
                }
            }
        }

        yield return new WaitForSeconds(boardConfig.refillDuration);

        var ctx = new BoardContext(Grid, null, currentLevel.columns, currentLevel.rows);
        BoardVisualizer.UpdateAllIcons(ctx, currentLevel);

        if (DeadlockSolver.IsDeadlocked(ctx))
        {
            StartCoroutine(ShuffleBoard());
        }
        else
        {
            _isProcessing = false;
        }
    }

    // Smart shuffle to resolve deadlocks
    private IEnumerator ShuffleBoard()
    {
        yield return new WaitForSeconds(boardConfig.shuffleStepDelay);

        List<Tile> allTiles = new List<Tile>();
        foreach (var t in Grid) if (t != null) allTiles.Add(t);

        var ctx = new BoardContext(Grid, allTiles, currentLevel.columns, currentLevel.rows);
        bool isSolvable = DeadlockSolver.IsSolvable(allTiles);

        // Phase 1: Injection - If unsolvable, inject a needed color
        if (!isSolvable)
        {
            if (allTiles.Count >= 2)
            {
                Tile source, target;
                if (DeadlockSolver.TryFindInjectionCandidates(ctx, boardConfig.maxCalculationAttempts, out source, out target))
                {
                    int targetType = source.ItemType;
                    TileSkin targetSkin = boardConfig.tileSkins[targetType];

                    target.AnimateColorChange(targetType, targetSkin, boardConfig.injectionDuration);

                    yield return new WaitForSeconds(boardConfig.injectionDuration + boardConfig.shuffleStepDelay);

                    if (!DeadlockSolver.IsDeadlocked(ctx))
                    {
                        BoardVisualizer.UpdateAllIcons(ctx, currentLevel);
                        _isProcessing = false;
                        yield break;
                    }
                }
            }
            else
            {
                _isProcessing = false;
                Debug.LogWarning("ShuffleBoard: Not enough tiles for injection.");
                yield break;
            }
        }

        // Phase 2: Shuffle - Randomize positions
        DeadlockSolver.ShuffleList(allTiles);

        int idx = 0;
        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                if (Grid[x, y] != null)
                {
                    Tile tile = allTiles[idx++];
                    Grid[x, y] = tile;
                    tile.x = x; tile.y = y;

                    Vector3 targetPos = GetWorldPosition(x, y);
                    tile.AnimateMove(targetPos, boardConfig.shuffleMoveDuration, boardConfig.shuffleEase);
                    tile.GetComponent<SpriteRenderer>().sortingOrder = (currentLevel.rows + y) * 10;
                }
            }
        }
        yield return new WaitForSeconds(boardConfig.shuffleMoveDuration);

        // Phase 3: Force Match - Ensure playability if deadlock persists
        if (DeadlockSolver.IsDeadlocked(ctx))
        {
            Tile tileA, tileB, slot1, slot2;

            if (DeadlockSolver.TryFindForceMatchCandidates(ctx, boardConfig.maxCalculationAttempts,
                out tileA, out tileB, out slot1, out slot2))
            {
                float dur = boardConfig.swapDuration;
                Ease swapEase = boardConfig.swapEase;

                PerformLogicalSwap(tileA, slot1);
                if (tileB == slot1) tileB = tileA;
                PerformLogicalSwap(tileB, slot2);

                tileA.AnimateMove(GetWorldPosition(tileA.x, tileA.y), dur, swapEase);
                slot1.AnimateMove(GetWorldPosition(slot1.x, slot1.y), dur, swapEase);
                tileB.AnimateMove(GetWorldPosition(tileB.x, tileB.y), dur, swapEase);
                slot2.AnimateMove(GetWorldPosition(slot2.x, slot2.y), dur, swapEase);

                yield return new WaitForSeconds(dur + boardConfig.shuffleStepDelay);
            }
        }

        BoardVisualizer.UpdateAllIcons(ctx, currentLevel);
        _isProcessing = false;
    }

    private void PerformLogicalSwap(Tile t1, Tile t2)
    {
        if (t1 == t2) return;
        int tempX = t1.x; int tempY = t1.y;
        Grid[t1.x, t1.y] = t2; Grid[t2.x, t2.y] = t1;
        t1.x = t2.x; t1.y = t2.y;
        t2.x = tempX; t2.y = tempY;
        t1.GetComponent<SpriteRenderer>().sortingOrder = (currentLevel.rows + t1.y) * 10;
        t2.GetComponent<SpriteRenderer>().sortingOrder = (currentLevel.rows + t2.y) * 10;
    }

    private void AdjustCamera(float boardWidth, float boardHeight)
    {
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        float aspectRatio = (float)Screen.width / Screen.height;

        float verticalSize = boardHeight / 2f + boardConfig.cameraMargin;
        float horizontalSize = (boardWidth / 2f + boardConfig.cameraMargin) / aspectRatio;

        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    private Vector2 GetWorldPosition(int x, int y)
    {
        float targetX = -((currentLevel.columns * boardConfig.tileSize + (currentLevel.columns - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + x * (boardConfig.tileSize + boardConfig.padding);
        float targetY = -((currentLevel.rows * boardConfig.tileSize + (currentLevel.rows - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + y * (boardConfig.tileSize + boardConfig.padding);
        return new Vector2(targetX, targetY);
    }

    public void SwitchLevel(LevelData newLevel)
    {
        StopAllCoroutines();
        DOTween.KillAll();
        _isProcessing = false;

        if (Grid != null)
        {
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    if (Grid[x, y] != null)
                    {
                        _pool.Release(Grid[x, y]);
                        Grid[x, y] = null;
                    }
                }
            }
        }
        currentLevel = newLevel;
        GenerateBoard();
    }
}