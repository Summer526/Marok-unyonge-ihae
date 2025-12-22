using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Managers")]
    public GameManager gameManager;
    public GridManager gridManager;

    [Header("Panels")]
    public GameObject panelMainMenu;
    public GameObject panelInGame;
    public GameObject panelInventory;
    public GameObject panelSettings;
    public GameObject panelGameOver;
    public GameObject panelShop;
    public GameObject panelHowToPlay; 

    [Header("In-Game Info UI")]
    public TMP_Text txtStage;
    public TMP_Text txtGold;
    public TMP_Text txtSwapCount;   // 남은 이동 횟수 숫자만
    public Button btnLeftElement;
    public Button btnRightElement;
    public Button btnConfirm;
    public Button btnOpenInventoryInGame;

    [Header("Game Over UI")]
    public TMP_Text txtGameOverStage; // "Stage X" 만 표기
    public TMP_Text txtGameOverGrade;
    public Button btnRetry;           // 게임 오버 – 다시하기 버튼
    public Button btnReturnToMain;    // 게임 오버 – 메인으로 버튼

    [Header("Main Menu Buttons")]
    public Button btnNewGame;         // 메인메뉴 – 새 게임
    public Button btnHowToPlay;       // 메인메뉴 – 플레이하는 법
    public Button btnSettings;        // 메인메뉴 – 설정
    public Button btnQuit;            // 메인메뉴 – 나가기
    public Button btnCloseHowToPlay;

    [Header("Shop UI")]
    public Button btnRerollShop;
    public TMP_Text txtRerollCost;
    public Button btnCloseShop;
    public TMP_Text txtShopGold;
    [System.Serializable]
    public class ShopItemSlot
    {
        public GameObject slotRoot;          // 슬롯 전체 오브젝트
        public Image imgIcon;                // 아이템 아이콘
        public TMP_Text txtName;             // 아이템 이름
        public TMP_Text txtDescription;      // 아이템 설명
        public TMP_Text txtPrice;            // 가격
        public Button btnBuy;                // 구매 버튼
        public GameObject soldOutOverlay;    // 품절 표시 (옵션)
    }
    public ShopItemSlot[] shopItemSlots;

    [Header("Inventory UI")]
    public Button btnCloseInventory;
    public TMP_Text txtInventoryItemName;     
    public TMP_Text txtInventoryItemDescription;
    public GameObject inventoryItemSlotPrefab;
    public Transform inventoryItemContainer;

    [Header("Editor Helper")]
    [SerializeField] private int slotCountToGenerate = 10;  // 생성할 슬롯 개수
    [ContextMenu("Generate Inventory Slots")]
    private void GenerateInventorySlots()
    {
        if (inventoryItemContainer == null)
        {
            Debug.LogError("Inventory Item Container가 할당되지 않았습니다!");
            return;
        }

        if (inventoryItemSlotPrefab == null)
        {
            Debug.LogError("Inventory Item Slot Prefab이 할당되지 않았습니다!");
            return;
        }

        // 기존 슬롯 전부 삭제
        while (inventoryItemContainer.childCount > 0)
        {
            DestroyImmediate(inventoryItemContainer.GetChild(0).gameObject);
        }

        // 새로 생성
        for (int i = 0; i < slotCountToGenerate; i++)
        {
#if UNITY_EDITOR
            GameObject slot = UnityEditor.PrefabUtility.InstantiatePrefab(inventoryItemSlotPrefab, inventoryItemContainer) as GameObject;
#else
        GameObject slot = Instantiate(inventoryItemSlotPrefab, inventoryItemContainer);
#endif
            slot.name = $"Slot_{i:00}";
        }

        Debug.Log($"{slotCountToGenerate}개의 슬롯 생성 완료!");
    }
    [System.Serializable]
    public class InventoryItemSlot
    {
        public GameObject slotRoot;          // 슬롯 전체 오브젝트
        public Image imgIcon;                // 아이템 아이콘
        public TMP_Text txtCount;            // 소유 개수
        public Button btnSlot;
    }

    [Header("Element Select UI")]
    public Image imgCurrentElement;

    [System.Serializable]
    public class ElementSpriteConfig
    {
        public ElementType element;
        public GameObject prefab; // 이 속성 타일 프리팹 (SpriteRenderer 포함)
    }

    public ElementSpriteConfig[] elementSprites;

    [Tooltip("플레이어가 좌/우 버튼으로 순환해서 선택할 수 있는 속성 순서")]
    public ElementType[] selectableElements =
    {
        ElementType.Wind,
        ElementType.Fire,
        ElementType.Lightning,
        ElementType.Water,
        ElementType.Earth,
        ElementType.Light,
        ElementType.Dark,
        ElementType.Heal,
        ElementType.Shield
    };

    private int currentElementIndex = 0;
    public ElementType CurrentElement
    {
        get
        {
            if (selectableElements == null || selectableElements.Length == 0)
                return ElementType.Wind;

            if (currentElementIndex < 0)
                currentElementIndex = 0;
            if (currentElementIndex >= selectableElements.Length)
                currentElementIndex = selectableElements.Length - 1;

            return selectableElements[currentElementIndex];
        }
    }

    [Header("Settings UI")]
    public Slider sliderBGM;
    public Slider sliderSE;
    public Button btnSettingsQuit;  
    public Button btnSettingsClose;
    private int lastBGMStage = -1;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
    }

    private void Start()
    {
        // 기본은 메인 메뉴부터
        ShowMainMenuPanel();
        UpdateElementDisplay();
        UpdateSwapCountUI();

        SetupButtonCallbacks();
        InitAudioSliders();

        if (sliderBGM != null)
        {
            sliderBGM.onValueChanged.RemoveAllListeners();
            sliderBGM.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sliderSE != null)
        {
            sliderSE.onValueChanged.RemoveAllListeners();
            sliderSE.onValueChanged.AddListener(OnSEVolumeChanged);
        }
    }

    void SetupButtonCallbacks()
    {
        // === 메인 메뉴 버튼들 ===
        if (btnNewGame != null)
        {
            btnNewGame.onClick.RemoveAllListeners();
            btnNewGame.onClick.AddListener(OnClickStartGame);
        }

        if (btnHowToPlay != null)
        {
            btnHowToPlay.onClick.RemoveAllListeners();
            btnHowToPlay.onClick.AddListener(OnClickHowToPlay);
        }

        if (btnCloseHowToPlay != null)
        {
            btnCloseHowToPlay.onClick.RemoveAllListeners();
            btnCloseHowToPlay.onClick.AddListener(OnClickCloseHowToPlay);
        }

        if (btnSettings != null)
        {
            btnSettings.onClick.RemoveAllListeners();
            btnSettings.onClick.AddListener(OnClickOpenSettingsFromMain);
        }

        if (btnQuit != null)
        {
            btnQuit.onClick.RemoveAllListeners();
            btnQuit.onClick.AddListener(OnClickQuitFromMain);
        }

        // === 게임 오버 버튼들 ===
        if (btnRetry != null)
        {
            btnRetry.onClick.RemoveAllListeners();
            btnRetry.onClick.AddListener(OnClickRetry);
        }

        if (btnReturnToMain != null)
        {
            btnReturnToMain.onClick.RemoveAllListeners();
            btnReturnToMain.onClick.AddListener(OnClickReturnToMain);
        }

        // === 설정 패널 버튼들 ===
        if (btnSettingsQuit != null)
        {
            btnSettingsQuit.onClick.RemoveAllListeners();
            btnSettingsQuit.onClick.AddListener(OnClickQuitFromSettings);
        }

        if (btnSettingsClose != null)
        {
            btnSettingsClose.onClick.RemoveAllListeners();
            btnSettingsClose.onClick.AddListener(OnClickCloseSettingsPanel);
        }
        if (btnLeftElement != null)
        {
            btnLeftElement.onClick.RemoveAllListeners();
            btnLeftElement.onClick.AddListener(OnClickPrevElement);
        }

        if (btnRightElement != null)
        {
            btnRightElement.onClick.RemoveAllListeners();
            btnRightElement.onClick.AddListener(OnClickNextElement);
        }

        if (btnConfirm != null)
        {
            btnConfirm.onClick.RemoveAllListeners();
            btnConfirm.onClick.AddListener(OnClickConfirm);
        }

        if (btnOpenInventoryInGame != null)
        {
            btnOpenInventoryInGame.onClick.RemoveAllListeners();
            btnOpenInventoryInGame.onClick.AddListener(OnClickOpenInventory);
        }

        // Shop
        if (btnRerollShop != null)
        {
            btnRerollShop.onClick.RemoveAllListeners();
            btnRerollShop.onClick.AddListener(OnClickRerollShop);
        }
        if (btnCloseShop != null)
        {
            btnCloseShop.onClick.RemoveAllListeners();
            btnCloseShop.onClick.AddListener(OnClickCloseShop);
        }

        //Inventory
        if (btnCloseInventory != null)
        {
            btnCloseInventory.onClick.RemoveAllListeners();
            btnCloseInventory.onClick.AddListener(OnClickCloseInventory);
        }
    }
    // =========================
    // BGM 헬퍼
    // =========================

    void PlayBattleBGMForStage(int stage)
    {
        if (AudioManager.Instance == null)
            return;

        if (stage >= 60)
        {
            AudioManager.Instance.PlayBGM("BGM_Battle_60~");
        }
        else if (stage >= 30)
        {
            AudioManager.Instance.PlayBGM("BGM_Battle_30~");
        }
        else
        {
            AudioManager.Instance.PlayBGM("BGM_Battle_~30");
        }
    }

    // =========================
    // 패널 ON / OFF 공통
    // =========================

    void SetAllPanelsOff()
    {
        if (panelMainMenu) panelMainMenu.SetActive(false);
        if (panelInGame) panelInGame.SetActive(false);
        if (panelInventory) panelInventory.SetActive(false);
        if (panelSettings) panelSettings.SetActive(false);
        if (panelGameOver) panelGameOver.SetActive(false);
        if (panelShop) panelShop.SetActive(false);
        if (panelHowToPlay) panelHowToPlay.SetActive(false);
    }

    public void ShowMainMenuPanel()
    {
        SetAllPanelsOff();
        if (panelMainMenu) panelMainMenu.SetActive(true);

        // 메인 메뉴 BGM
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM("BGM_MainMenu");
    }

    // 다른 스크립트에서 호출하는 이름 그대로
    public void ShowInGamePanel()
    {
        SetAllPanelsOff();
        if (panelInGame) panelInGame.SetActive(true);
    }


    public void ShowGameOverPanel(int stage, string grade)
    {
        SetAllPanelsOff();
        if (panelGameOver) panelGameOver.SetActive(true);
        UpdateGameOverInfo(stage, grade);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM("BGM_GameOver");
    }

    // 파라미터 없는 버전 - 다른 곳에서 호출용
    public void ShowGameOverPanel()
    {
        SetAllPanelsOff();
        if (panelGameOver) panelGameOver.SetActive(true);

        if (gameManager != null)
        {
            string grade = GetGradeForStage(gameManager.stage);
            UpdateGameOverInfo(gameManager.stage, grade);
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM("BGM_GameOver");
    }

    void UpdateGameOverInfo(int stage, string grade)
    {
        if (txtGameOverStage)
            txtGameOverStage.text = $"Stage {stage}";

        if (txtGameOverGrade)
            txtGameOverGrade.text = grade;
    }

    string GetGradeForStage(int stageNum)
    {
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

    // Shop 패널 – 이름 맞춰서 구현
    public void ShowShopPanel()
    {
        if (panelShop)
            panelShop.SetActive(true);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM("BGM_Shop");
    }

    public void HideShopPanel()
    {
        if (panelShop)
            panelShop.SetActive(false);
        if (gameManager != null)
        {
            PlayBattleBGMForStage(gameManager.stage);
        }
    }

    public void ToggleInventoryPanel(bool show)
    {
        if (panelInventory)
            panelInventory.SetActive(show);
    }

    public void ToggleSettingsPanel(bool show)
    {
        if (panelSettings)
            panelSettings.SetActive(show);
    }

    public void ToggleHowToPlayPanel(bool show)
    {
        if (panelHowToPlay)
            panelHowToPlay.SetActive(show);
    }
    void PlayButtonSE()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE("Button");
    }
    // =========================
    // 인게임 정보 갱신
    // =========================

    // GameManager.InitGame() 끝에서 호출용
    public void OnGameStarted(int stage, int killCount, int gold)
    {
        ShowInGamePanel();
        RefreshInGameInfo(stage, killCount, gold);
        UpdateSwapCountUI();
        UpdateElementDisplay();
    }

    public void RefreshInGameInfo(int stage, int killCount, int gold)
    {
        Debug.Log($"RefreshInGameInfo: Stage={stage}, Gold={gold}");

        if (txtStage != null)
        {
            txtStage.text = stage.ToString();
            Debug.Log($"Stage 텍스트 업데이트: {stage}");
        }
        else
        {
            Debug.LogWarning("txtStage가 null!");
        }

        if (txtGold != null)
        {
            txtGold.text = gold.ToString();
            Debug.Log($"Gold 텍스트 업데이트: {gold}");
        }
        else
        {
            Debug.LogWarning("txtGold가 null!");
        }


        int stageGroup = stage <= 30 ? 1 : (stage <= 60 ? 2 : 3);
        int lastGroup = lastBGMStage <= 30 ? 1 : (lastBGMStage <= 60 ? 2 : 3);

        if (stageGroup != lastGroup)
        {
            PlayBattleBGMForStage(stage);
            lastBGMStage = stage;
        }
    }

    // === 네가 말한 그대로: 인자 없는 버전, 남은 횟수만 숫자로 ===
    public void UpdateSwapCountUI()
    {
        if (txtSwapCount == null || gridManager == null)
            return;
        int remaining = gridManager.GetRemainingSwaps();
        txtSwapCount.text = remaining.ToString();
    }
    public void OnClickCloseHowToPlay()
    {
        PlayButtonSE();
        ToggleHowToPlayPanel(false);
    }
    // =========================
    // 메인 메뉴 / 게임오버 버튼
    // =========================

    // 메인메뉴 – 게임 시작 버튼
    public void OnClickStartGame()
    {
        PlayButtonSE();

        if (gameManager == null) return;

        gameManager.InitGame();
    }

    // 메인메뉴 – 게임하는 법 버튼
    public void OnClickHowToPlay()
    {
        PlayButtonSE();

        if (panelHowToPlay == null) return;

        bool nextState = !panelHowToPlay.activeSelf;
        ToggleHowToPlayPanel(nextState);
    }

    // 메인메뉴 – 나가기 버튼
    public void OnClickQuitFromMain()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 메인메뉴 – 설정 버튼
    public void OnClickOpenSettingsFromMain()
    {
        PlayButtonSE();

        ToggleSettingsPanel(true);
    }

    // 게임오버 – 다시하기 버튼
    public void OnClickRetry()
    {
        PlayButtonSE();

        if (gameManager == null) return;

        gameManager.InitGame();
    }

    // 게임오버 – 메인으로 버튼
    public void OnClickReturnToMain()
    {
        PlayButtonSE();

        ShowMainMenuPanel();
    }

    // 인게임 – 인벤토리 버튼
    public void OnClickOpenInventory()
    {
        Debug.Log("OnClickOpenInventory 호출됨!");

        PlayButtonSE();
        UpdateInventoryUI();
        ToggleInventoryPanel(true);
    }

    public void OnClickCloseInventory()
    {
        PlayButtonSE();

        ToggleInventoryPanel(false);
    }
    public void UpdateInventoryUI()
    {
        ItemManager itemMgr = FindObjectOfType<ItemManager>();
        if (itemMgr == null) return;

        // ★ 기존 슬롯 전부 삭제
        foreach (Transform child in inventoryItemContainer)
        {
            Destroy(child.gameObject);
        }

        List<ItemData> items = itemMgr.ownedItems;
        Dictionary<ItemType, List<ItemData>> groupedItems = new Dictionary<ItemType, List<ItemData>>();

        foreach (var item in items)
        {
            if (item == null) continue;
            if (!groupedItems.ContainsKey(item.itemType))
            {
                groupedItems[item.itemType] = new List<ItemData>();
            }
            groupedItems[item.itemType].Add(item);
        }

        // ★ 필요한 만큼만 슬롯 생성
        foreach (var kvp in groupedItems)
        {
            ItemData firstItem = kvp.Value[0];
            int count = kvp.Value.Count;

            // 프리팹 생성
            GameObject slotObj = Instantiate(inventoryItemSlotPrefab, inventoryItemContainer);

            // 컴포넌트 찾기
            Image imgIcon = slotObj.transform.Find("Canvas/Itme/IconImage")?.GetComponent<Image>();
            TMP_Text txtCount = slotObj.transform.Find("Canvas/Itme/Text (TMP)")?.GetComponent<TMP_Text>();
            Button btnSlot = slotObj.GetComponent<Button>();


            // 아이콘 설정
            if (imgIcon != null)
            {
                if (firstItem.icon != null)
                {
                    imgIcon.sprite = firstItem.icon;
                    imgIcon.enabled = true;
                }
                else
                {
                    imgIcon.enabled = false;
                }
            }

            // 개수 설정
            if (txtCount != null)
            {
                if (count > 1)
                {
                    txtCount.text = $"x{count}";
                    txtCount.gameObject.SetActive(true);
                }
                else
                {
                    txtCount.gameObject.SetActive(false);
                }
            }

            // 버튼 클릭 이벤트
            if (btnSlot != null)
            {
                ItemData itemToShow = firstItem;
                btnSlot.onClick.AddListener(() => ShowInventoryItemInfo(itemToShow));
            }
        }

        ClearInventoryItemInfo();
    }
    void ShowInventoryItemInfo(ItemData item)
    {
        if (item == null) return;

        if (txtInventoryItemName != null)
        {
            txtInventoryItemName.text = item.displayName;
        }

        if (txtInventoryItemDescription != null)
        {
            txtInventoryItemDescription.text = item.description;
        }
    }

    void ClearInventoryItemInfo()
    {
        if (txtInventoryItemName != null)
        {
            txtInventoryItemName.text = "아이템을 선택하세요";
        }

        if (txtInventoryItemDescription != null)
        {
            txtInventoryItemDescription.text = "";
        }
    }
    // 인게임 – 설정 버튼
    public void OnClickOpenSettingsInGame()
    {
        PlayButtonSE();

        ToggleSettingsPanel(true);
    }

    public void OnClickCloseSettings()
    {
        PlayButtonSE();

        ToggleSettingsPanel(false);
    }

    // =========================
    // 설정 패널 – BGM / SE 슬라이더
    // =========================

    void InitAudioSliders()
    {
        if (sliderBGM != null)
        {
            float v = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
            sliderBGM.value = v;
        }
        if (sliderSE != null)
        {
            float v = PlayerPrefs.GetFloat("SEVolume", 1.0f);
            sliderSE.value = v;
        }
    }

    // 슬라이더 OnValueChanged(float) 에 연결
    public void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    // 슬라이더 OnValueChanged(float) 에 연결
    public void OnSEVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SEVolume", value);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSEVolume(value);
    }

    // 설정 패널 안의 "나가기" 버튼 – 메인 메뉴로
    public void OnClickQuitFromSettings()
    {
        PlayButtonSE();
        ShowMainMenuPanel();
    }
    public void OnClickCloseSettingsPanel()
    {
        PlayButtonSE();
        ToggleSettingsPanel(false);
    }
    // =========================
    // 속성 선택 / 아이콘 표시
    // =========================

    public void OnClickPrevElement()
    {
        if (selectableElements == null || selectableElements.Length == 0)
            return;

        currentElementIndex--;
        if (currentElementIndex < 0)
            currentElementIndex = selectableElements.Length - 1;

        UpdateElementDisplay();
    }

    public void OnClickNextElement()
    {
        if (selectableElements == null || selectableElements.Length == 0)
            return;

        currentElementIndex++;
        if (currentElementIndex >= selectableElements.Length)
            currentElementIndex = 0;

        UpdateElementDisplay();
    }

    void UpdateElementDisplay()
    {
        if (imgCurrentElement != null)
        {
            Color color;
            Sprite s = GetSpriteForElement(CurrentElement, out color);

            imgCurrentElement.sprite = s;

            if (s != null)
            {
                imgCurrentElement.enabled = true;
                imgCurrentElement.color = color;
            }
            else
            {
                imgCurrentElement.enabled = false;
            }
        }

        if (gridManager != null)
        {
            gridManager.HighlightLongestChain(CurrentElement);
        }
    }

    Sprite GetSpriteForElement(ElementType elem, out Color color)
    {
        color = Color.white;

        if (elementSprites == null)
            return null;

        for (int i = 0; i < elementSprites.Length; i++)
        {
            var cfg = elementSprites[i];
            if (cfg == null) continue;
            if (cfg.element != elem) continue;
            if (cfg.prefab == null) continue;

            var sr = cfg.prefab.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) continue;

            color = sr.color;
            return sr.sprite;
        }

        return null;
    }

    // =========================
    // 턴 액션 버튼
    // =========================

    public void OnClickConfirm()
    {
        AudioManager.Instance.PlaySE("Attack");
        if (gameManager == null) return;
        if (!gameManager.isPlayerTurn) return;

        ElementType elem = CurrentElement;

        if (elem == ElementType.Heal)
        {
            gameManager.PlayerHeal();
        }
        else if (elem == ElementType.Shield)
        {
            gameManager.PlayerShield();
        }
        else
        {
            gameManager.PlayerAttack(elem);
        }
    }

    public void OnClickCloseShop()
    {
        PlayButtonSE();

        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            shopManager.CloseShop();
        }
    }

    public void OnClickRerollShop()
    {

        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            shopManager.TryRerollShop();
        }
    }

    public void OnClickBuyShopItem(int slotIndex)
    {
        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager == null) return;

        List<ItemData> items = shopManager.GetCurrentShopItems();
        if (slotIndex < 0 || slotIndex >= items.Count) return;

        ItemData item = items[slotIndex];
        if (item != null)
        {
            shopManager.TryBuyItem(item);
        }
    }

    public void UpdateShopUI()
    {
        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager == null) return;

        // 보유 골드 표시
        if (txtShopGold != null && gameManager != null)
        {
            txtShopGold.text = string.Format("{0}G", gameManager.gold);
        }

        // 리롤 비용 표시
        if (txtRerollCost != null)
        {
            int cost = shopManager.GetRerollCost();
            txtRerollCost.text = string.Format("{0}G", cost);
        }

        // 아이템 슬롯 갱신
        List<ItemData> items = shopManager.GetCurrentShopItems();

        for (int i = 0; i < shopItemSlots.Length; i++)
        {
            ShopItemSlot slot = shopItemSlots[i];
            if (slot == null || slot.slotRoot == null) continue;

            if (i < items.Count)
            {
                // 아이템 있음
                ItemData item = items[i];
                slot.slotRoot.SetActive(true);

                // 구매 여부 체크
                bool isPurchased = shopManager.IsItemPurchased(item);

                // 아이콘
                if (slot.imgIcon != null)
                {
                    slot.imgIcon.sprite = item.icon;
                    slot.imgIcon.enabled = (item.icon != null);
                }

                // 이름
                if (slot.txtName != null)
                {
                    slot.txtName.text = item.displayName;
                }

                // 설명
                if (slot.txtDescription != null)
                {
                    slot.txtDescription.text = item.description;
                }

                // 가격
                if (slot.txtPrice != null)
                {
                    int price = shopManager.GetItemPrice(item);
                    slot.txtPrice.text = string.Format("{0}G", price);
                }

                // 구매 버튼
                if (slot.btnBuy != null)
                {
                    if (!isPurchased)
                    {
                        slot.btnBuy.onClick.RemoveAllListeners();
                        int index = i;
                        slot.btnBuy.onClick.AddListener(() => OnClickBuyShopItem(index));

                        // ★ 구매 완료된 것만 비활성화, 나머지는 항상 활성화
                        slot.btnBuy.interactable = true;
                    }
                    else
                    {
                        slot.btnBuy.interactable = false;
                    }
                }

                // 품절 표시 (구매한 아이템만)
                if (slot.soldOutOverlay != null)
                {
                    slot.soldOutOverlay.SetActive(isPurchased);
                }
            }
            else
            {
                // 아이템 없음 - 슬롯 숨김
                slot.slotRoot.SetActive(false);
            }
        }
    }
}
