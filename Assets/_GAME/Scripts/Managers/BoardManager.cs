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
            createFunc: () => Instantiate(boardConfig.tilePrefab, transform).GetComponent<Tile>(),
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
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) HandleInput();
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
                if (connectedTiles.Count >= 2) StartCoroutine(ExplodeTiles(connectedTiles));
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
        UpdateBoardVisuals();

        if (IsDeadlocked())
        {
            _isProcessing = true;
            StartCoroutine(ShuffleBoard());
        }
    }

    // --- GÖRSEL UPDATE ---
    private void UpdateBoardVisuals()
    {
        bool[,] visited = new bool[currentLevel.columns, currentLevel.rows];
        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                Tile tile = Grid[x, y];
                if (tile != null && !visited[x, y])
                {
                    List<Tile> group = MatchFinder.FindMatches(tile, Grid, currentLevel.rows, currentLevel.columns);
                    foreach (Tile t in group) visited[t.x, t.y] = true;

                    foreach (Tile member in group)
                    {
                        if (group.Count > currentLevel.conditionC) member.SetVisualState(3);
                        else if (group.Count > currentLevel.conditionB) member.SetVisualState(2);
                        else if (group.Count > currentLevel.conditionA) member.SetVisualState(1);
                        else member.SetVisualState(0);
                    }
                }
            }
        }
    }

    // --- EXPLODE & GRAVITY ---
    private IEnumerator ExplodeTiles(List<Tile> matches)
    {
        _isProcessing = true;
        foreach (Tile tile in matches) Grid[tile.x, tile.y] = null;

        foreach (Tile tile in matches)
        {
            tile.transform.DOScale(Vector3.zero, boardConfig.explodeDuration)
                .SetEase(boardConfig.explodeEase)
                .OnComplete(() => _pool.Release(tile));
        }

        yield return new WaitForSeconds(boardConfig.explodeDuration);
        StartCoroutine(ApplyGravity());
    }

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

                        Vector3 targetPos = GetWorldPosition(x, writeY);
                        int newSortingOrder = (currentLevel.rows + writeY) * 10;
                        tile.GetComponent<SpriteRenderer>().sortingOrder = newSortingOrder;

                        tile.transform.DOMove(targetPos, boardConfig.fallDuration)
                            .SetEase(boardConfig.fallEase, boardConfig.gravityOvershoot);
                    }
                    writeY++;
                }
            }
        }

        if (boardConfig.refillDelay > 0f) yield return new WaitForSeconds(boardConfig.refillDelay);
        else yield return null;

        StartCoroutine(FillBoard());
    }

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

                    newTile.transform.DOMove(targetPos, boardConfig.refillDuration)
                        .SetEase(boardConfig.refillEase, boardConfig.refillOvershoot);
                }
            }
        }

        yield return new WaitForSeconds(boardConfig.refillDuration);
        UpdateBoardVisuals();

        if (IsDeadlocked())
        {
            StartCoroutine(ShuffleBoard());
        }
        else
        {
            _isProcessing = false;
        }
    }

    // --- AKILLI VE SIRALI SHUFFLE SÝSTEMÝ ---
    private IEnumerator ShuffleBoard()
    {
        yield return new WaitForSeconds(boardConfig.shuffleStepDelay);

        // --- FAZ 1: ANALÝZ ---
        Dictionary<int, int> colorCounts = new Dictionary<int, int>();
        List<Tile> allTiles = new List<Tile>();

        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                if (Grid[x, y] != null)
                {
                    allTiles.Add(Grid[x, y]);
                    if (!colorCounts.ContainsKey(Grid[x, y].ItemType)) colorCounts[Grid[x, y].ItemType] = 0;
                    colorCounts[Grid[x, y].ItemType]++;
                }
            }
        }

        bool isSolvable = false;
        foreach (var count in colorCounts.Values) if (count >= 2) { isSolvable = true; break; }

        // --- FAZ 2: RENK ENJEKSÝYONU ---
        if (!isSolvable)
        {
            if (allTiles.Count >= 2)
            {
                // Renkleri deðiþtir
                yield return StartCoroutine(ProcessInjection(allTiles));

                // --- OPTÝMÝZASYON BURADA ---
                // Renk deðiþimi sonrasý hemen kontrol et.
                // allTiles[0] ve [1] genelde komþu olduðu için (0,0 ve 0,1)
                // renk deðiþimi anýnda bir eþleþme yaratýr.
                if (!IsDeadlocked())
                {
                    // Sorun çözüldü! Karýþtýrmaya (Shuffle) gerek kalmadý.
                    // Görselleri güncelle ve oyunu aç.
                    UpdateBoardVisuals();
                    _isProcessing = false;
                    yield break; // Coroutine'den çýk
                }
            }
            else
            {
                _isProcessing = false;
                yield break;
            }
        }

        // --- FAZ 3: KARIÞTIRMA (Sadece deadlock hala devam ediyorsa buraya gelir) ---
        yield return StartCoroutine(ProcessShuffle(allTiles));

        // --- FAZ 4: SON KONTROL VE FORCE MATCH ---
        if (IsDeadlocked())
        {
            yield return StartCoroutine(ProcessForceMatch(allTiles));
        }

        UpdateBoardVisuals();
        _isProcessing = false;
    }

    // --- ALT COROUTINE: RENK ENJEKSÝYONU ---
    private IEnumerator ProcessInjection(List<Tile> allTiles)
    {
        float animDuration = 0.4f;

        Tile sourceTile = null;
        Tile targetNeighbor = null;

        // 1. Rastgele bir taþ ve onun geçerli bir komþusunu bul
        int attempts = 0;
        while (attempts < 50) // Deneme sayýsýný biraz arttýrdým garanti olsun diye
        {
            sourceTile = allTiles[Random.Range(0, allTiles.Count)];

            List<Tile> validNeighbors = new List<Tile>();

            // 4 Yöne Bak
            if (sourceTile.x < currentLevel.columns - 1)
            {
                Tile t = Grid[sourceTile.x + 1, sourceTile.y];
                if (t != null) validNeighbors.Add(t);
            }
            if (sourceTile.x > 0)
            {
                Tile t = Grid[sourceTile.x - 1, sourceTile.y];
                if (t != null) validNeighbors.Add(t);
            }
            if (sourceTile.y < currentLevel.rows - 1)
            {
                Tile t = Grid[sourceTile.x, sourceTile.y + 1];
                if (t != null) validNeighbors.Add(t);
            }
            if (sourceTile.y > 0)
            {
                Tile t = Grid[sourceTile.x, sourceTile.y - 1];
                if (t != null) validNeighbors.Add(t);
            }

            if (validNeighbors.Count > 0)
            {
                targetNeighbor = validNeighbors[Random.Range(0, validNeighbors.Count)];
                break; // Ýkili bulundu!
            }

            attempts++;
        }

        // Fail-Safe: Eðer aþýrý þanssýzsak ve komþu bulamazsak (neredeyse imkansýz),
        // listenin baþýndaki iki taþý kullan.
        if (targetNeighbor == null && allTiles.Count >= 2)
        {
            sourceTile = allTiles[0];
            targetNeighbor = allTiles[1];
        }

        // 2. OPERASYON: Komþuyu, Kaynaðýn rengine boya
        if (sourceTile != null && targetNeighbor != null)
        {
            // Kaynak taþýn rengini öðren
            int targetType = sourceTile.ItemType;
            TileSkin targetSkin = boardConfig.tileSkins[targetType];

            // Sadece komþuyu deðiþtir (Kaynak sabit kalýr)
            targetNeighbor.AnimateColorChange(targetType, targetSkin, animDuration);
        }

        // Animasyon süresi + StepDelay kadar bekle
        yield return new WaitForSeconds(animDuration + boardConfig.shuffleStepDelay);
    }

    // --- ALT COROUTINE: KARIÞTIRMA ---
    private IEnumerator ProcessShuffle(List<Tile> allTiles)
    {
        // Matematiksel karýþtýrma
        for (int i = 0; i < allTiles.Count; i++)
        {
            Tile temp = allTiles[i];
            int rnd = Random.Range(i, allTiles.Count);
            allTiles[i] = allTiles[rnd];
            allTiles[rnd] = temp;
        }

        // Görsel yerleþtirme
        int idx = 0;
        for (int x = 0; x < currentLevel.columns; x++)
        {
            for (int y = 0; y < currentLevel.rows; y++)
            {
                if (Grid[x, y] != null)
                {
                    Tile tile = allTiles[idx++];
                    Grid[x, y] = tile;
                    tile.x = x;
                    tile.y = y;

                    Vector3 targetPos = GetWorldPosition(x, y);
                    tile.transform.DOMove(targetPos, 0.5f).SetEase(Ease.InOutQuad);
                    tile.GetComponent<SpriteRenderer>().sortingOrder = (currentLevel.rows + y) * 10;
                }
            }
        }

        // Hareket süresi (0.5f) + StepDelay kadar bekle
        yield return new WaitForSeconds(0.5f + boardConfig.shuffleStepDelay);
    }

    // --- ALT COROUTINE: FORCE MATCH ---
    // --- GÜNCELLENMÝÞ ALT COROUTINE: RANDOM FORCE MATCH ---
    private IEnumerator ProcessForceMatch(List<Tile> allTiles)
    {
        // 1. En az 2 tane olan bir renk bul
        int targetColor = -1;
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var t in allTiles)
        {
            if (!counts.ContainsKey(t.ItemType)) counts[t.ItemType] = 0;
            counts[t.ItemType]++;
            if (counts[t.ItemType] >= 2) { targetColor = t.ItemType; break; }
        }

        // 2. Bu renkteki iki taþý bul (Taþýnacak olanlar)
        Tile tileA = allTiles.Find(t => t.ItemType == targetColor);
        Tile tileB = allTiles.FindLast(t => t.ItemType == targetColor);

        // 3. Rastgele Hedef Konumlar Seç (Slot1 ve Slot2)
        Tile slot1 = null;
        Tile slot2 = null;

        int attempts = 0;
        while (attempts < 50)
        {
            // Rastgele bir ana slot seç
            int randX = Random.Range(0, currentLevel.columns);
            int randY = Random.Range(0, currentLevel.rows);
            slot1 = Grid[randX, randY];

            if (slot1 == null) { attempts++; continue; }

            // Slot1'in geçerli komþularýný bul
            List<Tile> validNeighbors = new List<Tile>();

            // Sað
            if (randX < currentLevel.columns - 1)
            {
                Tile t = Grid[randX + 1, randY];
                if (t != null) validNeighbors.Add(t);
            }
            // Sol
            if (randX > 0)
            {
                Tile t = Grid[randX - 1, randY];
                if (t != null) validNeighbors.Add(t);
            }
            // Üst
            if (randY < currentLevel.rows - 1)
            {
                Tile t = Grid[randX, randY + 1];
                if (t != null) validNeighbors.Add(t);
            }
            // Alt
            if (randY > 0)
            {
                Tile t = Grid[randX, randY - 1];
                if (t != null) validNeighbors.Add(t);
            }

            // Eðer komþu bulduysak birini seç ve çýk
            if (validNeighbors.Count > 0)
            {
                slot2 = validNeighbors[Random.Range(0, validNeighbors.Count)];
                break;
            }
            attempts++;
        }

        // Fail-Safe: Eðer rastgele yer bulamazsak (çok zor ama) yine sol alta koy.
        if (slot1 == null || slot2 == null)
        {
            slot1 = Grid[0, 0];
            slot2 = Grid[0, 1];
        }

        // 4. SWAP ÝÞLEMÝ VE ANÝMASYON
        if (slot1 != null && slot2 != null)
        {
            float swapDuration = 0.5f;

            // Mantýksal Swap: tileA -> slot1
            PerformLogicalSwap(tileA, slot1);

            // Eðer tileB þans eseri slot1'in kendisi idiyse, 
            // tileA ile yer deðiþtirdiði için artýk tileA'nýn eski yerindedir.
            if (tileB == slot1) tileB = tileA;

            // Mantýksal Swap: tileB -> slot2
            PerformLogicalSwap(tileB, slot2);

            // Görsel Animasyon (DOMove) - Yeni koordinatlara git
            // Not: PerformLogicalSwap ile x,y deðerleri güncellendiði için 
            // GetWorldPosition artýk yeni hedefi verir.
            tileA.transform.DOMove(GetWorldPosition(tileA.x, tileA.y), swapDuration).SetEase(Ease.OutBack);
            slot1.transform.DOMove(GetWorldPosition(slot1.x, slot1.y), swapDuration).SetEase(Ease.OutBack);

            tileB.transform.DOMove(GetWorldPosition(tileB.x, tileB.y), swapDuration).SetEase(Ease.OutBack);
            slot2.transform.DOMove(GetWorldPosition(slot2.x, slot2.y), swapDuration).SetEase(Ease.OutBack);

            // Animasyon süresi + Delay kadar bekle
            yield return new WaitForSeconds(swapDuration + boardConfig.shuffleStepDelay);
        }
    }

    // Yardýmcý: Sadece data deðiþimi yapar (Animasyon yok)
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

    private bool IsDeadlocked()
    {
        for (int x = 0; x < currentLevel.columns; x++)
            for (int y = 0; y < currentLevel.rows; y++)
            {
                Tile t = Grid[x, y];
                if (t != null)
                {
                    if (x < currentLevel.columns - 1) { Tile r = Grid[x + 1, y]; if (r != null && r.ItemType == t.ItemType) return false; }
                    if (y < currentLevel.rows - 1) { Tile up = Grid[x, y + 1]; if (up != null && up.ItemType == t.ItemType) return false; }
                }
            }
        return true;
    }

    private void AdjustCamera(float boardWidth, float boardHeight)
    {
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = boardHeight / 2f + 1f;
        float horizontalSize = (boardWidth / 2f + 1f) / aspectRatio;
        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    private Vector2 GetWorldPosition(int x, int y)
    {
        float targetX = -((currentLevel.columns * boardConfig.tileSize + (currentLevel.columns - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + x * (boardConfig.tileSize + boardConfig.padding);
        float targetY = -((currentLevel.rows * boardConfig.tileSize + (currentLevel.rows - 1) * boardConfig.padding) / 2f) + boardConfig.tileSize / 2f + y * (boardConfig.tileSize + boardConfig.padding);
        return new Vector2(targetX, targetY);
    }
}