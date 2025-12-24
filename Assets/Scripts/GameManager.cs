using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public PlayerStats player;
    public GameObject playerPrefab;  // ← 추가
    public Transform playerSpawnPoint;
    public GameObject enemyPrefab;
    public Transform enemySpawnPoint;

    [Header("Background Settings")]
    public GameObject[] stageBackgrounds;

    [Header("Enemy Settings")]
    public EnemySpawnData[] enemySpawnList;
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;

        [Tooltip("높을수록 자주 등장 (1.0 = 일반, 0.1 = 희귀)")]
        public float spawnWeight = 1.0f;
    }

    public ElementType[] enemyElements = new ElementType[]
    {
        ElementType.Wind,
        ElementType.Fire,
        ElementType.Lightning,
        ElementType.Water,
        ElementType.Earth,
        ElementType.Light,
        ElementType.Dark
    };

    [Header("Game State")]
    public GameMode currentGameMode = GameMode.Normal;
    public int stage = 1;
    public int killCount = 0;
    public int gold = 0;
    public int boardSize = 2;
    public bool isPlayerTurn = true;

    [Header("Turn Timing")]
    public float actionDelay = 0.5f;      // 액션 후 대기 시간
    public float enemyTurnDelay = 1.0f;   // 적 턴 전 대기

    private EnemyStats currentEnemy;
    private GridManager gridManager;
    private ComboManager comboManager;
    private ItemManager itemManager;
    private ShopManager shopManager;
    private UIManager uiManager;
    float GetChainMultiplier(int chainCount)
    {
        if (chainCount <= 1) return 1f;

        // 1 + k * (10-1) = 5 → k = 4/9
        float k = 4f / 9f;
        float mult = 1f + k * (chainCount - 1);

        // 10체인 이상은 5배로 캡
        return Mathf.Min(mult, 5f);
    }
    [Header("Board Expansion")]
    private int[] boardWidths = { 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7 };
    private int[] boardHeights = { 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
    private int expansionIndex = 0;

    [Header("Tutorial")]
    public ItemData howToPlayItem;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // 씬 안에서 다른 매니저들 찾아오기
            gridManager = FindObjectOfType<GridManager>();
            comboManager = FindObjectOfType<ComboManager>();
            itemManager = FindObjectOfType<ItemManager>();
            shopManager = FindObjectOfType<ShopManager>();
            uiManager = FindObjectOfType<UIManager>();
            if (shopManager != null && itemManager != null)
            {
                shopManager.Initialize(this, itemManager);
            }

            InitializeBackgrounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
       
    }

    public void InitGame()
    {
        stage = 1;
        killCount = 0;
        gold = 0;

        SetupPlayer();

        // 보드 초기화
        if (gridManager != null)
        {
            boardSize = gridManager.size;
            gridManager.InitializeGrid(boardSize, boardSize);
        }

        if (itemManager != null && gridManager != null)
        {
            itemManager.Initialize(gridManager);

            // ★ How To Play 아이템 기본 지급
            if (howToPlayItem != null)
            {
                itemManager.AddItem(howToPlayItem);
            }
        }


        if (comboManager != null && itemManager != null)
        {
            comboManager.Initialize(itemManager);
        }

        UpdateBackground();
        SpawnEnemy();

        if (uiManager != null)
        {
            uiManager.OnGameStarted(stage, killCount, gold);
        }

        UpdateAllUI();
        StartPlayerTurn();
    }
    void SetupPlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
            Debug.Log("기존 플레이어 삭제");
        }

        // ★ 플레이어 새로 생성
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            Debug.Log($"플레이어 생성 위치: {playerSpawnPoint.position}");
            GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            playerObj.name = "Player";
            player = playerObj.GetComponent<PlayerStats>();
            Debug.Log($"플레이어 생성 완료: {player != null}");
        }
        else
        {
            Debug.LogError($"playerPrefab: {playerPrefab != null}, playerSpawnPoint: {playerSpawnPoint != null}");
        }

        // 플레이어 스탯 초기화
        if (player != null)
        {
            player.hasLastStandUsed = false;
            player.lastStandTriggeredThisHit = false;
            player.shield = 0f;
            player.UpdateStatsForStage(stage);
        }
    }
    void SpawnEnemy()
    {
        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
        }
        EnemyStats.availableElements = enemyElements;

        // ★ 현재 스테이지에 맞는 프리팹 선택
        GameObject selectedPrefab = SelectRandomEnemyPrefab();

        if (selectedPrefab == null)
        {
            Debug.LogError("등장 가능한 몹이 없습니다!");
            return;
        }

        GameObject enemyObj = Instantiate(selectedPrefab, enemySpawnPoint.position, Quaternion.identity);
        currentEnemy = enemyObj.GetComponent<EnemyStats>();
        currentEnemy.InitializeRandom(stage);

        Debug.Log($"스테이지 {stage} - {currentEnemy.typeName} 생성: {currentEnemy.elementType}, HP: {currentEnemy.maxHP:F1}, ATK: {currentEnemy.atk:F1}");
    }
    GameObject SelectRandomEnemyPrefab()
    {
        if (enemySpawnList == null || enemySpawnList.Length == 0)
            return null;

        // ★ 현재 스테이지에 등장 가능한 프리팹만 필터링
        List<EnemySpawnData> availableEnemies = new List<EnemySpawnData>();

        foreach (var data in enemySpawnList)
        {
            if (data.enemyPrefab == null) continue;

            EnemyStats stats = data.enemyPrefab.GetComponent<EnemyStats>();
            if (stats == null) continue;

            // minStage ~ maxStage 범위 체크
            bool inRange = stage >= stats.minStage && (stats.maxStage == 0 || stage <= stats.maxStage);

            if (inRange)
            {
                availableEnemies.Add(data);
            }
        }

        if (availableEnemies.Count == 0)
        {
            Debug.LogWarning($"스테이지 {stage}에 등장 가능한 몹이 없음!");
            return null;
        }

        // 전체 가중치 합계
        float totalWeight = 0f;
        foreach (var data in availableEnemies)
        {
            totalWeight += data.spawnWeight;
        }

        if (totalWeight <= 0f)
            return availableEnemies[0].enemyPrefab;

        // 가중치 랜덤 선택
        float random = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var data in availableEnemies)
        {
            cumulative += data.spawnWeight;
            if (random <= cumulative)
            {
                return data.enemyPrefab;
            }
        }

        return availableEnemies[0].enemyPrefab;
    }
    
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        Debug.Log("플레이어 턴 시작");
    }

    // === 플레이어 액션 ===

    // 선택한 속성으로 공격
    public void PlayerAttack(ElementType element)
    {
        if (!isPlayerTurn)
            return;

        if (gridManager == null || player == null || currentEnemy == null)
            return;

        List<Tile> chain = gridManager.GetLongestChain(element);
        if (chain == null || chain.Count == 0)
        {
            if (comboManager != null)
            {
                comboManager.OnNonAttack();

                if (uiManager != null)
                {
                    uiManager.UpdateComboUI(comboManager.comboStreak);
                }
            }
            gridManager.ResetSwapCount();
            StartEnemyTurn();
            return;
        }

        // ★ 콤보 계산 - 여기 한 번만!
        float comboMult = 1f;
        if (comboManager != null)
        {
            comboManager.OnAttack(element);  // ★ 이것만 남기기
            comboMult = comboManager.GetComboMultiplier();

            if (uiManager != null)
            {
                uiManager.UpdateComboUI(comboManager.comboStreak);
            }
        }

        int chainCount = chain.Count;
        if (itemManager != null && itemManager.HasAllSevenOrbs())
        {
            chainCount = 10;
            Debug.Log("세븐 오브 컬렉션! 최소 10체인 보장");
        }

        if (itemManager != null)
        {
            chainCount = itemManager.GetEffectiveChainCount(chainCount);
        }

        float baseAtk = player.GetCurrentATK();

        if (itemManager != null)
        {
            baseAtk = itemManager.ApplyAttackBonus(baseAtk);
        }

        float chainMult = GetChainMultiplier(chainCount);
        ElementType enemyElement = currentEnemy.elementType;
        float affinityMult = GetElementAffinity(element, enemyElement);

        float damage = baseAtk * chainMult * comboMult * affinityMult;

        Debug.Log(
            $"공격: 공격속성={element}, 적속성={enemyElement}, 체인={chainCount}, 콤보배수={comboMult:F2}, 상성배수={affinityMult:F2}, 최종데미지={damage:F1}"
        );

        if (itemManager != null)
        {
            itemManager.ApplyBarrierOnDamage(player, damage);
        }

        gridManager.ApplyAdditionalRandomRemove();
        gridManager.FillEmptyTiles();
        gridManager.ResetSwapCount();

        UpdateAllUI();
        StartCoroutine(DelayedEndPlayerTurn(PlayerActionType.Attack, damage, 0f));
    }

    // Heal 속성으로 회복
    public void PlayerHeal()
    {
        if (!isPlayerTurn)
            return;

        if (gridManager == null || player == null)
            return;

        List<Tile> chain = gridManager.GetLongestChain(ElementType.Heal);
        if (chain == null || chain.Count == 0)
        {
            Debug.Log("Heal 체인이 없음 - 턴 패스");

            if (comboManager != null)
            {
                comboManager.OnNonAttack();
            }
            if (uiManager != null)
            {
                uiManager.UpdateComboUI(comboManager.comboStreak);
            }
            gridManager.ResetSwapCount();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        int chainCount = chain.Count;
        float healAmount = player.maxHP * 0.05f * chainCount;

        // 치유 강화 적용
        if (itemManager != null && itemManager.hasHealBoost)
        {
            healAmount += player.maxHP * 0.05f;
        }

        float missingHP = player.maxHP - player.currentHP;
        if (missingHP < 0f) missingHP = 0f;

        float healApplied = Mathf.Min(healAmount, missingHP);

        if (comboManager != null)
        {
            comboManager.OnNonAttack();

            // ★ 콤보 UI 업데이트
            if (uiManager != null)
            {
                uiManager.UpdateComboUI(comboManager.comboStreak);
            }
        }

        gridManager.RemoveTiles(chain);
        gridManager.ApplyAdditionalRandomRemove();
        gridManager.FillEmptyTiles();
        gridManager.ResetSwapCount();

        UpdateAllUI();
        StartCoroutine(DelayedEndPlayerTurn(PlayerActionType.Heal, 0f, healApplied));
    }

    public void PlayerShield()
    {
        if (!isPlayerTurn)
            return;

        if (gridManager == null || player == null)
            return;

        List<Tile> chain = gridManager.GetLongestChain(ElementType.Shield);
        if (chain == null || chain.Count == 0)
        {
            Debug.Log("Shield 체인이 없음 - 턴 패스");

            if (comboManager != null)
            {
                comboManager.OnNonAttack();

                // ★ 콤보 UI 즉시 업데이트
                if (uiManager != null)
                {
                    uiManager.UpdateComboUI(comboManager.comboStreak);
                }
            }

            gridManager.ResetSwapCount();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        int chainCount = chain.Count;

        float shieldAmount = player.maxHP * 0.03f * chainCount;
        player.AddShield(shieldAmount);

        Debug.Log($"쉴드 획득: 체인={chainCount}, 쉴드+{shieldAmount:F1} (현재 {player.shield:F1})");

        if (comboManager != null)
        {
            comboManager.OnNonAttack();

            // ★ 콤보 UI 즉시 업데이트
            if (uiManager != null)
            {
                uiManager.UpdateComboUI(comboManager.comboStreak);
            }
        }

        gridManager.RemoveTiles(chain);
        gridManager.ApplyAdditionalRandomRemove();
        gridManager.FillEmptyTiles();
        gridManager.ResetSwapCount();

        UpdateAllUI();
        StartCoroutine(DelayedEndPlayerTurn(PlayerActionType.Shield, 0f, 0f));
    }

    // === UI 버튼용 래퍼 ===

    // 인스펙터에서 int 파라미터로 속성 인덱스를 넘겨서 사용
    public void OnAttackButton(int elementIndex)
    {
        ElementType element = (ElementType)elementIndex;
        PlayerAttack(element);
    }

    public void OnHealButton()
    {
        PlayerHeal();
    }

    IEnumerator DelayedEndPlayerTurn(PlayerActionType actionType, float damage, float heal)
    {
        yield return new WaitForSeconds(actionDelay);

        if (actionType == PlayerActionType.Attack)
        {
            currentEnemy.TakeDamage(damage);
            Debug.Log($"몹에게 {damage} 데미지!");
            UpdateAllUI();

            if (currentEnemy.IsDead())
            {
                yield return new WaitForSeconds(0.5f);
                OnEnemyKilled();
                yield break;
            }
        }
        else if (actionType == PlayerActionType.Heal)
        {
            player.Heal(heal);
            Debug.Log($"플레이어 {heal} 회복!");
            UpdateAllUI();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("Heal");
        }
        else if (actionType == PlayerActionType.Shield)
        {
            Debug.Log($"플레이어 쉴드 턴 종료 (현재 쉴드: {player.shield:F1})");
            UpdateAllUI();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("MakeShield");
        }

        StartCoroutine(DelayedEnemyTurn());
    }

    IEnumerator DelayedEnemyTurn()
    {
        yield return new WaitForSeconds(enemyTurnDelay);
        StartEnemyTurn();
    }

    void OnEnemyKilled()
    {
        killCount++;
        stage++;

        // 골드 획득 (난이도 구간별로 다른 기본 골드)
        int baseGold;
        if (stage <= 30)
        {
            baseGold = 10;
        }
        else if (stage <= 60)
        {
            baseGold = 15;
        }
        else
        {
            baseGold = 20;
        }
        int totalGold = baseGold + currentEnemy.goldBonus;
        if (itemManager != null)
        {
            gold += itemManager.GetGoldOnKill(totalGold);
        }
        else
        {
            gold += baseGold;
        }

        Debug.Log($"몹 처치! 킬카운트: {killCount}, 골드: {gold}");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE("MobDie");

        player.OnEnemyKilled();
        player.UpdateStatsForStage(stage);

        if (uiManager != null)
            uiManager.RefreshInGameInfo(stage, killCount, gold);

        if (killCount % 3 == 0)
        {
            // 2×2 → 2×3 → 3×3 → 3×4 → ...
            if (boardSize == 2 && gridManager.height == 2)
            {
                gridManager.ResizeGrid(2, 3); // 2×3
            }
            else if (gridManager.width == gridManager.height)
            {
                // 정사각형이면 세로 +1
                gridManager.ResizeGrid(gridManager.width, gridManager.height + 1);
            }
            else
            {
                // 직사각형이면 가로 +1 (정사각형 만들기)
                gridManager.ResizeGrid(gridManager.width + 1, gridManager.height);
            }

            boardSize = Mathf.Max(gridManager.width, gridManager.height);

            if (boardSize >= 7)
            {
                gridManager.ResizeGrid(7, 7); // 최대 7×7
            }
        }

        if (killCount % 15 == 0)
        {
            OpenShop();
        }
        UpdateBackground();
        SpawnEnemy();
        UpdateAllUI();
        StartPlayerTurn();
    }


    public void StartEnemyTurn()
    {
        isPlayerTurn = false;
        Debug.Log("몹 턴 시작");

        bool hasLastStandItem = (itemManager != null && itemManager.hasLastStand);
        currentEnemy.AttackPlayer(player, hasLastStandItem);
        UpdateAllUI();
        // 라스트 스탠드 발동했으면 콤보 끊기
        if (player.lastStandTriggeredThisHit && comboManager != null)
        {
            comboManager.ResetCombo();

            // ★ 콤보 UI 업데이트
            if (uiManager != null)
            {
                uiManager.UpdateComboUI(comboManager.comboStreak);
            }
        }

        if (player.IsDead())
        {
            GameOver();
            return;
        }
        StartPlayerTurn();
    }

    float GetElementAffinity(ElementType attack, ElementType defense)
    {
        float strong = 1.25f; // 유리할 때 배수
        float weak = 0.3f;   // 불리할 때 배수

        // Heal, Shield는 상성 없음
        if (attack == ElementType.Heal || defense == ElementType.Heal ||
            attack == ElementType.Shield || defense == ElementType.Shield)
            return 1f;

        // 5각형 상성: 불 → 바람 → 땅 → 번개 → 물 → 불
        switch (attack)
        {
            case ElementType.Fire:
                // 불 > 바람, 불 < 물
                if (defense == ElementType.Wind) return strong;
                if (defense == ElementType.Water) return weak;
                break;

            case ElementType.Wind:
                // 바람 > 땅, 바람 < 불
                if (defense == ElementType.Earth) return strong;
                if (defense == ElementType.Fire) return weak;
                break;

            case ElementType.Earth:
                // 땅 > 번개, 땅 < 바람
                if (defense == ElementType.Lightning) return strong;
                if (defense == ElementType.Wind) return weak;
                break;

            case ElementType.Lightning:
                // 번개 > 물, 번개 < 땅
                if (defense == ElementType.Water) return strong;
                if (defense == ElementType.Earth) return weak;
                break;

            case ElementType.Water:
                // 물 > 불, 물 < 번개
                if (defense == ElementType.Fire) return strong;
                if (defense == ElementType.Lightning) return weak;
                break;

            case ElementType.Light:
                // 빛 > 어둠
                if (defense == ElementType.Dark) return strong;
                break;

            case ElementType.Dark:
                // 어둠 > 빛
                if (defense == ElementType.Light) return strong;
                break;
        }

        // 위에 안 걸리면 기본 1배 (상성 없음)
        return 1f;
    }
    void OpenShop()
    {
        Debug.Log("상점 오픈!");
        if (shopManager != null)
        {
            shopManager.OpenShop();
        }
    }

    void GameOver()
    {
        Debug.Log("게임 오버!");
        isPlayerTurn = false;

        string grade = GetGradeForStage(stage);

        // 100스테이지 클리어 시 무한모드 해금
        if (stage >= 100 && currentGameMode == GameMode.Normal)
        {
            PlayerPrefs.SetInt("EndlessModeUnlocked", 1);
            PlayerPrefs.Save();
            Debug.Log("무한모드 해금!");
        }

        uiManager?.ShowGameOverPanel(stage, grade);
    }
    string GetGradeForStage(int stageNum)
    {
        if (currentGameMode == GameMode.Endless)
        {
            // 무한모드 등급
            if (stageNum >= 300) return "SSS";
            if (stageNum >= 250) return "SS";
            if (stageNum >= 200) return "S";

            // 200층 이전: 10단계 구간 (100~200)
            if (stageNum >= 190) return "A+";
            if (stageNum >= 180) return "A";
            if (stageNum >= 170) return "B+";
            if (stageNum >= 160) return "B";
            if (stageNum >= 150) return "C+";
            if (stageNum >= 140) return "C";
            if (stageNum >= 130) return "D+";
            if (stageNum >= 120) return "D";
            if (stageNum >= 110) return "F+";
            return "F";
        }
        else
        {
            // 일반모드 등급 (기존)
            if (stageNum >= 100) return "S+";
            if (stageNum >= 90) return "A+";
            if (stageNum >= 80) return "A";
            if (stageNum >= 70) return "B+";
            if (stageNum >= 60) return "B";
            if (stageNum >= 50) return "C+";
            if (stageNum >= 40) return "C";
            if (stageNum >= 30) return "D+";
            if (stageNum >= 20) return "D";
            return "F";
        }
    }
    public bool IsEndlessModeUnlocked()
    {
        return PlayerPrefs.GetInt("EndlessModeUnlocked", 0) == 1;
    }
    public void UpdateAllUI()
    {
        if (player != null)
        {
            WorldUnitHUD playerHUD = player.GetComponentInChildren<WorldUnitHUD>();
            if (playerHUD != null)
            {
                Debug.Log($"플레이어 HUD 업데이트: HP {player.currentHP}/{player.maxHP}");
                playerHUD.UpdateUI();
            }
            else
            {
                Debug.LogWarning("플레이어 HUD를 찾을 수 없음!");
            }
        }

        if (currentEnemy != null)
        {
            WorldUnitHUD enemyHUD = currentEnemy.GetComponentInChildren<WorldUnitHUD>();
            if (enemyHUD != null)
            {
                Debug.Log($"적 HUD 업데이트: HP {currentEnemy.currentHP}/{currentEnemy.maxHP}");
                enemyHUD.UpdateUI();
            }
            else
            {
                Debug.LogWarning("적 HUD를 찾을 수 없음!");
            }
        }
    }
    void InitializeBackgrounds()
    {
        if (stageBackgrounds == null || stageBackgrounds.Length == 0)
            return;

        // 모든 배경 비활성화
        foreach (var bg in stageBackgrounds)
        {
            if (bg != null)
                bg.SetActive(false);
        }
    }

    // 스테이지에 맞는 배경 활성화
    void UpdateBackground()
    {
        if (stageBackgrounds == null || stageBackgrounds.Length == 0)
            return;

        int bgIndex;

        // 100층 이하: 10스테이지마다 배경 변경 (0~9)
        // 101층 이상: 11번째 배경 고정 (10)
        if (stage <= 100)
        {
            bgIndex = (stage - 1) / 10;
        }
        else
        {
            bgIndex = 10; // 11번째 배경
        }

        // 배열 범위 체크
        if (bgIndex < 0 || bgIndex >= stageBackgrounds.Length)
            bgIndex = stageBackgrounds.Length - 1; // 마지막 배경 유지

        // 모든 배경 끄고, 해당 배경만 켜기
        for (int i = 0; i < stageBackgrounds.Length; i++)
        {
            if (stageBackgrounds[i] != null)
            {
                stageBackgrounds[i].SetActive(i == bgIndex);
            }
        }
    }
}