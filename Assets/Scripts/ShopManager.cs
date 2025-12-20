using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public List<ItemData> allItems = new List<ItemData>();
    public int itemCount = 3;

    private GameManager gameManager;
    private ItemManager itemManager;
    private UIManager uiManager;
    private List<ItemData> currentShopItems = new List<ItemData>();
    private List<ItemData> purchasedItems = new List<ItemData>(); // 이번 상점에서 구매한 아이템

    // 리롤 관련
    private int rerollCount = 0;
    public int baseRerollCost = 10;

    // 아이템별 가중치
    private Dictionary<ItemType, float> itemWeights = new Dictionary<ItemType, float>()
    {
        { ItemType.AttributeResonance, 2f },
        { ItemType.AttributeOrb, 2f },
        { ItemType.ComboKeeper, 1f },
        { ItemType.ChainBooster, 1f },
        { ItemType.HealBoost, 1f },
        { ItemType.Barrier, 1f },
        { ItemType.ShopDiscount, 1f },
        { ItemType.LastStand, 1f },
        { ItemType.ManaBracelet, 1f },
        { ItemType.ManaNecklace, 1f },
        { ItemType.ManaRing, 1f },
        { ItemType.MadisHand, 1f }
    };

    public void Initialize(GameManager gm, ItemManager itemMgr)
    {
        gameManager = gm;
        itemManager = itemMgr;

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    public void OpenShop()
    {
        Debug.Log("=== 상점 오픈 ===");

        // 리롤 카운트 초기화
        rerollCount = 0;
        purchasedItems.Clear();

        GenerateShopItems();

        // UI로 상점 표시
        if (uiManager != null)
        {
            uiManager.ShowShopPanel();
            uiManager.UpdateShopUI();
        }
    }

    void GenerateShopItems()
    {
        // 구매 가능한 아이템 필터링 (maxStack 체크)
        List<ItemData> availableItems = allItems
            .Where(item => itemManager.CanBuyMore(item))
            .ToList();

        if (availableItems.Count == 0)
        {
            Debug.Log("구매 가능한 아이템 없음");
            currentShopItems.Clear();
            return;
        }

        // 가중치 기반 랜덤 선택
        currentShopItems.Clear();
        int count = Mathf.Min(itemCount, availableItems.Count);

        for (int i = 0; i < count; i++)
        {
            ItemData selected = SelectWeightedRandom(availableItems);
            if (selected != null)
            {
                currentShopItems.Add(selected);
                availableItems.Remove(selected); // 같은 상점에 중복 방지
            }
        }

        // 로그 출력
        for (int i = 0; i < currentShopItems.Count; i++)
        {
            ItemData item = currentShopItems[i];
            int price = GetItemPrice(item);
            Debug.Log($"{i + 1}. {item.displayName} - {price}G");
            Debug.Log($"   {item.description}");
        }
    }

    ItemData SelectWeightedRandom(List<ItemData> items)
    {
        if (items == null || items.Count == 0)
            return null;

        // 각 아이템의 가중치 합계 계산
        float totalWeight = 0f;
        foreach (var item in items)
        {
            float weight = itemWeights.ContainsKey(item.itemType) ? itemWeights[item.itemType] : 1f;
            totalWeight += weight;
        }

        // 가중치 기반 랜덤 선택
        float random = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var item in items)
        {
            float weight = itemWeights.ContainsKey(item.itemType) ? itemWeights[item.itemType] : 1f;
            cumulative += weight;

            if (random <= cumulative)
            {
                // AttributeOrb인 경우, 7종 중 랜덤 선택
                if (item.itemType == ItemType.AttributeOrb)
                {
                    return SelectRandomOrb(items);
                }

                // AttributeResonance인 경우, 21종 중 랜덤 선택
                if (item.itemType == ItemType.AttributeResonance)
                {
                    return SelectRandomResonance(items);
                }

                return item;
            }
        }

        return items[0];
    }

    ItemData SelectRandomOrb(List<ItemData> allItems)
    {
        // AttributeOrb 타입인 아이템들만 필터링
        List<ItemData> orbs = allItems.FindAll(i => i.itemType == ItemType.AttributeOrb);

        if (orbs.Count == 0)
            return null;

        // 7종 중 랜덤
        return orbs[Random.Range(0, orbs.Count)];
    }

    ItemData SelectRandomResonance(List<ItemData> allItems)
    {
        // AttributeResonance 타입인 아이템들만 필터링
        List<ItemData> resonances = allItems.FindAll(i => i.itemType == ItemType.AttributeResonance);

        if (resonances.Count == 0)
            return null;

        // 21종 중 랜덤
        return resonances[Random.Range(0, resonances.Count)];
    }

    public int GetRerollCost()
    {
        return baseRerollCost + (rerollCount * 10);
    }

    public bool TryRerollShop()
    {
        int cost = GetRerollCost();

        if (gameManager.gold < cost)
        {
            Debug.Log($"골드 부족! 리롤 비용: {cost}, 보유: {gameManager.gold}");
            return false;
        }

        gameManager.gold -= cost;
        rerollCount++;

        Debug.Log($"상점 리롤! 비용: {cost}, 남은 골드: {gameManager.gold}");
        AudioManager.Instance.PlaySE("Buy");
        // 리롤 시 구매 목록도 초기화
        purchasedItems.Clear();
        GenerateShopItems();

        // UI 갱신
        if (uiManager != null)
        {
            uiManager.UpdateShopUI();
            uiManager.RefreshInGameInfo(gameManager.stage, gameManager.killCount, gameManager.gold);
        }

        return true;
    }


    public int GetItemPrice(ItemData item)
    {
        if (itemManager == null)
            return item.basePrice;

        return itemManager.GetDiscountedPrice(item.basePrice);
    }

    public bool TryBuyItem(ItemData item)
    {
        if (!itemManager.CanBuyMore(item))
        {
            Debug.Log($"{item.displayName}은(는) 더 이상 구매할 수 없습니다!");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("CantBuy");
            return false;
        }

        int price = GetItemPrice(item);

        if (gameManager.gold >= price)
        {
            gameManager.gold -= price;
            itemManager.AddItem(item);
            purchasedItems.Add(item);

            Debug.Log($"{item.displayName} 구매 완료! 남은 골드: {gameManager.gold}");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("Buy");

            if (uiManager != null)
            {
                uiManager.UpdateShopUI();
                uiManager.RefreshInGameInfo(gameManager.stage, gameManager.killCount, gameManager.gold);
            }

            return true;
        }
        else
        {
            Debug.Log($"골드 부족! 필요: {price}, 보유: {gameManager.gold}");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("CantBuy");
            return false;
        }
    }

    public void CloseShop()
    {
        currentShopItems.Clear();
        Debug.Log("상점 닫힘");

        if (uiManager != null)
        {
            uiManager.HideShopPanel();
        }
    }

    public List<ItemData> GetCurrentShopItems()
    {
        return currentShopItems;
    }
    public bool IsItemPurchased(ItemData item)
    {
        return purchasedItems.Contains(item);
    }
}