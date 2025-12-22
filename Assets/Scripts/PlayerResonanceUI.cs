using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerResonanceUI : MonoBehaviour
{
    [Header("Resonance Panels")]
    public GameObject panel1;
    public GameObject panel2;

    [Header("Panel1 Images")]
    public Image panel1_Image;      // Image
    public Image panel1_Image1;     // Image (1)
    public Image panel1_Image2;     // Image (2) - 배경용이면 안 건드려도 됨

    [Header("Panel2 Images")]
    public Image panel2_Image;      // Image
    public Image panel2_Image1;     // Image (1)
    public Image panel2_Image2;     // Image (2) - 배경용이면 안 건드려도 됨

    private ItemManager itemManager;
    private GridManager gridManager;

    void Start()
    {
        itemManager = FindObjectOfType<ItemManager>();
        gridManager = FindObjectOfType<GridManager>();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (itemManager == null)
        {
            itemManager = FindObjectOfType<ItemManager>();
            if (itemManager == null) return;
        }

        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        var activeResonances = itemManager.activeResonances;

        // 첫 번째 공명서
        if (activeResonances.Count > 0 && activeResonances[0] != null)
        {
            if (panel1 != null)
                panel1.SetActive(true);

            ItemData res1 = activeResonances[0];
            SetElementIcon(panel1_Image, res1.primaryElement);
            SetElementIcon(panel1_Image1, res1.secondaryElement);
        }
        else
        {
            if (panel1 != null)
                panel1.SetActive(false);
        }

        // 두 번째 공명서
        if (activeResonances.Count > 1 && activeResonances[1] != null)
        {
            if (panel2 != null)
                panel2.SetActive(true);

            ItemData res2 = activeResonances[1];
            SetElementIcon(panel2_Image, res2.primaryElement);
            SetElementIcon(panel2_Image1, res2.secondaryElement);
        }
        else
        {
            if (panel2 != null)
                panel2.SetActive(false);
        }
    }

    void SetElementIcon(Image img, ElementType element)
    {
        if (img == null) return;

        Sprite sprite;
        Color color;

        if (GetElementSprite(element, out sprite, out color))
        {
            img.sprite = sprite;
            img.color = color;
            img.enabled = true;
        }
        else
        {
            img.enabled = false;
        }
    }

    bool GetElementSprite(ElementType element, out Sprite sprite, out Color color)
    {
        sprite = null;
        color = Color.white;

        if (gridManager != null)
        {
            var config = gridManager.GetConfig(element);
            if (config != null && config.tilePrefab != null)
            {
                SpriteRenderer sr = config.tilePrefab.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sprite = sr.sprite;
                    color = sr.color;
                    return true;
                }
            }
        }

        return false;
    }
}