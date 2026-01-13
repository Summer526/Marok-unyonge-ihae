using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("References")]
    public Image iconImage;
    public TMP_Text countText;
    public Button button;

    private ItemData itemData;

    public void Setup(ItemData item, int count, System.Action<ItemData> onClickCallback)
    {
        itemData = item;

        // 아이콘 설정
        if (iconImage != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }

        // 개수 표시
        if (countText != null)
        {
            countText.text = $"x{count}";
            countText.gameObject.SetActive(true);
        }


        // 버튼 콜백
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke(itemData));
        }
    }
}