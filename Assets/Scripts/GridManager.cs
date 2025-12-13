using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int size = 2;
    public Tile[,] tiles;

    public int width = 2;
    public int height = 2;
    [Header("Grid")]
    public Transform gridParent;
    public float tileSpacing = 1.1f;
    public Vector3 gridCenter = Vector3.zero;

    [Header("Grid Visual")]
    public GameObject cellPrefab;   // 칸 배경 프리팹 (선택)
    public float cellZOffset = 1f;  // 타일보다 뒤로 가게 Z 오프셋
    
    [Header("기본 타일 프리팹 (예비용)")]
    public GameObject tilePrefab;   // elementConfigs에 해당 속성이 없을 때 사용

    [System.Serializable]
    public class ElementConfig
    {
        public ElementType element;      // Wind / Fire / ...
        public GameObject tilePrefab;    // 이 속성 전용 타일 프리팹 (Tile 컴포넌트 포함)
    }

    [Header("속성별 타일 프리팹 설정")]
    public ElementConfig[] elementConfigs;

    [Header("Preview Settings")]
    public bool showPreviewBounds = true;
    [Range(1, 7)]
    public int previewMaxSize = 7;
    public Color previewColor = new Color(1f, 1f, 1f, 0.25f);

    private Dictionary<ElementType, float> spawnWeights = new Dictionary<ElementType, float>();
    private ItemManager itemManager;

    private List<Tile> highlightedChain = new List<Tile>();

    private Tile selectedTile = null;

    private int swapCount = 0;
    public int maxSwapsPerTurn = 1;
    public int MAX_SWAPS = 3;
    private Tile dragStartTile = null;

    void Awake()
    {
        InitializeWeights();
    }

    void InitializeWeights()
    {
        // 기본 가중치 (모두 동일)
        foreach (ElementType type in System.Enum.GetValues(typeof(ElementType)))
        {
            spawnWeights[type] = 1f;
        }
    }

    public void InitializeGrid(int w, int h)
    {
        ClearHighlight();

        width = Mathf.Max(2, w);
        height = Mathf.Max(2, h);
        tiles = new Tile[width, height];

        InitializeWeights();
        if (itemManager == null)
        {
            itemManager = FindObjectOfType<ItemManager>();
        }

        // 기존 타일/칸 제거
        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                Destroy(gridParent.GetChild(i).gameObject);
            }
        }

        // 1) 칸 배경 먼저 생성
        if (cellPrefab != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 localPos = new Vector3(x * tileSpacing, y * tileSpacing, cellZOffset);
                    GameObject cell = Instantiate(cellPrefab, gridParent);
                    cell.transform.localPosition = localPos;
                }
            }
        }

        // 2) 실제 타일 생성
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnTileAt(x, y);
            }
        }

        // 3) 가운데 정렬
        CenterGrid();
        UpdateHighlightForCurrentElement();
    }

    public void ResizeGrid(int newWidth, int newHeight)
    {
        InitializeGrid(newWidth, newHeight);
    }


    void SpawnAllTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnTileAt(x, y);
            }
        }

        CenterGrid();
    }

    void SpawnTileAt(int x, int y)
    {
        // gridParent 기준 로컬 좌표
        Vector3 localPos = new Vector3(x * tileSpacing, y * tileSpacing, 0f);

        // 1) 어떤 속성의 타일을 뽑을지 먼저 결정
        ElementType randomType = GetRandomElement();

        // 2) 기본 프리팹으로 시작
        GameObject prefabToUse = tilePrefab;

        // 3) elementConfigs에 이 속성용 프리팹이 있으면 그걸 사용
        if (elementConfigs != null)
        {
            for (int i = 0; i < elementConfigs.Length; i++)
            {
                var cfg = elementConfigs[i];
                if (cfg != null && cfg.element == randomType && cfg.tilePrefab != null)
                {
                    prefabToUse = cfg.tilePrefab;
                    break;
                }
            }
        }

        // 4) 선택된 프리팹으로 타일 생성 (로컬 좌표로 배치)
        GameObject tileObj = Instantiate(prefabToUse, gridParent);
        tileObj.transform.localPosition = localPos;

        Tile tile = tileObj.GetComponent<Tile>();
        if (tile == null)
        {
            Debug.LogError($"타일 프리팹에 Tile 컴포넌트가 없습니다: {prefabToUse.name}");
            return;
        }

        tile.Initialize(this, new Vector2Int(x, y), randomType);
        tiles[x, y] = tile;
    }



    ElementType GetRandomElement()
    {
        // ItemManager가 있고 오브를 보유중이면 새로운 확률 계산 사용
        if (itemManager != null && itemManager.hasOrb)
        {
            // 모든 속성 타입을 배열로
            ElementType[] allElements = (ElementType[])System.Enum.GetValues(typeof(ElementType));

            // 각 속성의 확률 계산
            Dictionary<ElementType, float> probabilities = new Dictionary<ElementType, float>();
            float totalProb = 0f;

            foreach (ElementType elem in allElements)
            {
                float prob = itemManager.GetElementSpawnProbability(elem);
                probabilities[elem] = prob;
                totalProb += prob;
            }

            // 가중치 랜덤 선택
            float random = Random.Range(0f, totalProb);
            float cumulative = 0f;

            foreach (var kvp in probabilities)
            {
                cumulative += kvp.Value;
                if (random <= cumulative)
                {
                    return kvp.Key;
                }
            }
        }

        // 오브가 없으면 Shield/Heal 10% 고정, 나머지 균등 분배
        float rand = Random.Range(0f, 1f);

        if (rand < 0.1f)
            return ElementType.Shield;
        else if (rand < 0.2f)
            return ElementType.Heal;
        else
        {
            // 나머지 7개 중 하나 (각 11.43%)
            ElementType[] combatElements = new ElementType[]
            {
            ElementType.Wind,
            ElementType.Fire,
            ElementType.Lightning,
            ElementType.Water,
            ElementType.Earth,
            ElementType.Light,
            ElementType.Dark
            };

            return combatElements[Random.Range(0, combatElements.Length)];
        }
    }

    void CenterGrid()
    {
        if (gridParent == null)
            return;

        float offsetX = (width - 1) * tileSpacing * 0.5f;
        float offsetY = (height - 1) * tileSpacing * 0.5f;

        gridParent.position = gridCenter - new Vector3(offsetX, offsetY, 0f);
    }

    public void IncreaseElementWeight(ElementType element, float multiplier)
    {
        if (spawnWeights.ContainsKey(element))
        {
            spawnWeights[element] *= multiplier;
            Debug.Log($"{element} 출현률 증가: {spawnWeights[element]}");
        }
    }
    public ElementConfig GetConfig(ElementType type)
    {
        if (elementConfigs == null) return null;

        for (int i = 0; i < elementConfigs.Length; i++)
        {
            var cfg = elementConfigs[i];
            if (cfg != null && cfg.element == type)
                return cfg;
        }

        return null;
    }
    public void OnTileClicked(Tile tile)
    {
        if (swapCount >= MAX_SWAPS)
        {
            Debug.Log("더 이상 스왑 불가 (3회 제한)");
            return;
        }

        if (selectedTile == null)
        {
            selectedTile = tile;
            Debug.Log($"첫 번째 타일 선택: ({tile.gridPos.x}, {tile.gridPos.y})");
        }
        else
        {
            SwapTiles(selectedTile, tile);
            selectedTile = null;
            swapCount++;
            Debug.Log($"스왑 완료 ({swapCount}/{MAX_SWAPS})");

            // ★ 스왑 후 남은 횟수 UI 갱신
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateSwapCountUI();
            }
        }
    }


    public void SwapTiles(Tile a, Tile b)
    {
        // 위치와 속성 저장
        Vector2Int posA = a.gridPos;
        Vector2Int posB = b.gridPos;
        ElementType elemA = a.elementType;
        ElementType elemB = b.elementType;
        Vector3 worldPosA = a.transform.position;
        Vector3 worldPosB = b.transform.position;

        // 기존 타일 파괴
        Destroy(a.gameObject);
        Destroy(b.gameObject);

        // 새 타일 생성 (위치 바꿔서)
        tiles[posA.x, posA.y] = CreateTileAt(posA.x, posA.y, elemB, worldPosB);
        tiles[posB.x, posB.y] = CreateTileAt(posB.x, posB.y, elemA, worldPosA);

        UpdateHighlightForCurrentElement();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateSwapCountUI();
        }
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE("Swap");
    }
    Tile CreateTileAt(int x, int y, ElementType element, Vector3 worldPos)
    {
        // 1) 해당 속성의 프리팹 찾기
        GameObject prefabToUse = tilePrefab;

        if (elementConfigs != null)
        {
            foreach (var cfg in elementConfigs)
            {
                if (cfg != null && cfg.element == element && cfg.tilePrefab != null)
                {
                    prefabToUse = cfg.tilePrefab;
                    break;
                }
            }
        }

        // 2) 생성
        GameObject tileObj = Instantiate(prefabToUse, gridParent);
        tileObj.transform.position = worldPos;

        Tile tile = tileObj.GetComponent<Tile>();
        if (tile == null)
        {
            Debug.LogError($"타일 프리팹에 Tile 컴포넌트가 없습니다: {prefabToUse.name}");
            return null;
        }

        tile.Initialize(this, new Vector2Int(x, y), element);
        return tile;
    }

    public void ResetSwapCount()
    {
        swapCount = 0;
        selectedTile = null;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateSwapCountUI();
        }
    }

    public List<Tile> GetLongestChain(ElementType targetElement)
    {
        bool[,] visited = new bool[width, height];
        List<Tile> longestChain = new List<Tile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y] && tiles[x, y].elementType == targetElement)
                {
                    List<Tile> currentChain = new List<Tile>();
                    FloodFill(x, y, targetElement, visited, currentChain);

                    if (currentChain.Count > longestChain.Count)
                    {
                        longestChain = currentChain;
                    }
                }
            }
        }

        return longestChain;
    }

    void FloodFill(int x, int y, ElementType targetType, bool[,] visited, List<Tile> chain)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;
        if (visited[x, y])
            return;
        if (tiles[x, y].elementType != targetType)
            return;

        visited[x, y] = true;
        chain.Add(tiles[x, y]);

        FloodFill(x + 1, y, targetType, visited, chain);
        FloodFill(x - 1, y, targetType, visited, chain);
        FloodFill(x, y + 1, targetType, visited, chain);
        FloodFill(x, y - 1, targetType, visited, chain);
    }

    public void RemoveTiles(List<Tile> tilesToRemove)
    {
        foreach (Tile tile in tilesToRemove)
        {
            tiles[tile.gridPos.x, tile.gridPos.y] = null;
            Destroy(tile.gameObject);
        }
    }

    public void ApplyAdditionalRandomRemove()
    {
        // 보드 크기에 따른 제거 개수 (큰 쪽 기준)
        int boardMax = Mathf.Max(width, height);
        int removeCount = 0;

        switch (boardMax)
        {
            case 2: removeCount = 1; break;
            case 3: removeCount = 2; break;
            case 4: removeCount = 3; break;
            case 5: removeCount = 4; break;
            case 6: removeCount = 5; break;
            case 7: removeCount = 6; break;
        }

        List<Tile> remainingTiles = new List<Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] != null)
                {
                    remainingTiles.Add(tiles[x, y]);
                }
            }
        }

        for (int i = 0; i < removeCount && remainingTiles.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, remainingTiles.Count);
            Tile tileToRemove = remainingTiles[randomIndex];

            tiles[tileToRemove.gridPos.x, tileToRemove.gridPos.y] = null;
            Destroy(tileToRemove.gameObject);
            remainingTiles.RemoveAt(randomIndex);
        }
    }


    public void FillEmptyTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == null)
                {
                    SpawnTileAt(x, y);
                }
            }
        }
        UpdateHighlightForCurrentElement();
    }
    public void BeginDrag(Tile tile)
    {
        dragStartTile = tile;
    }

    public void EndDragAtPosition(Vector3 worldPos)
    {
        if (dragStartTile == null)
            return;

        // 마우스가 너무 안 움직였으면 드래그로 안 보고 취소
        float sqrDistance = (worldPos - dragStartTile.transform.position).sqrMagnitude;
        float minDragDistance = 0.3f; // 0.1f → 0.3f로 증가 (너무 민감하지 않게)
        if (sqrDistance < minDragDistance * minDragDistance)
        {
            dragStartTile = null;
            return;
        }

        // 드래그 방향 기준으로 이웃 타일 찾기
        Tile target = GetNeighborInDragDirection(dragStartTile, worldPos);

        if (target != null && target != dragStartTile)
        {
            OnTileClicked(dragStartTile);
            OnTileClicked(target);
        }

        dragStartTile = null;
    }

    // 드래그 방향으로 인접한 타일 하나 선택
    Tile GetNeighborInDragDirection(Tile from, Vector3 dragEndWorldPos)
    {
        Vector3 startPos = from.transform.position;
        Vector3 delta = dragEndWorldPos - startPos;

        if (Mathf.Abs(delta.x) < 0.01f && Mathf.Abs(delta.y) < 0.01f)
            return null;

        int dx = 0;
        int dy = 0;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            dx = delta.x > 0 ? 1 : -1;
        }
        else
        {
            dy = delta.y > 0 ? 1 : -1;
        }

        int targetX = from.gridPos.x + dx;
        int targetY = from.gridPos.y + dy;

        // 범위 체크
        if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height)
            return null;

        return tiles[targetX, targetY];
    }
    void OnDrawGizmosSelected()
    {
        if (!showPreviewBounds)
            return;

        int sizeToDraw = Mathf.Max(1, previewMaxSize);
        float half = (sizeToDraw - 1) * tileSpacing * 0.5f;

        // 셀 중심 시작 위치
        float startX = gridCenter.x - half;
        float startY = gridCenter.y - half;

        Gizmos.color = previewColor;

        for (int x = 0; x < sizeToDraw; x++)
        {
            for (int y = 0; y < sizeToDraw; y++)
            {
                Vector3 center = new Vector3(
                    startX + x * tileSpacing,
                    startY + y * tileSpacing,
                    0f
                );

                // 각 칸을 wire cube로 그려줌
                Gizmos.DrawWireCube(center, new Vector3(tileSpacing, tileSpacing, 0f));
            }
        }
    }
    public int GetRemainingSwaps()
    {
        int remaining = MAX_SWAPS - swapCount;
        if (remaining < 0) remaining = 0;
        return remaining;
    }
    // ★ 현재 하이라이트 전부 끄기
    void ClearHighlight()
    {
        if (highlightedChain != null)
        {
            foreach (var t in highlightedChain)
            {
                if (t != null)
                    t.SetHighlighted(false);
            }
        }
        highlightedChain = new List<Tile>();
    }

    // ★ 특정 속성의 최장 체인을 찾아서 하이라이트
    public void HighlightLongestChain(ElementType targetElement)
    {
        ClearHighlight();

        if (tiles == null) return;

        List<Tile> chain = GetLongestChain(targetElement);
        if (chain == null || chain.Count == 0) return;

        highlightedChain = chain;

        foreach (var t in highlightedChain)
        {
            if (t != null)
                t.SetHighlighted(true);
        }
    }

    // ★ UI에서 선택한 속성(CurrentElement)에 맞춰 자동으로 갱신
    void UpdateHighlightForCurrentElement()
    {
        if (UIManager.Instance == null) return;

        ElementType elem = UIManager.Instance.CurrentElement;
        HighlightLongestChain(elem);
    }
}