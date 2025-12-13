using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldUnitHUD : MonoBehaviour
{
    [Header("Owner")]
    public PlayerStats player;
    public EnemyStats enemy;

    [Header("HP UI")]
    public Image hpFillImage;
    public TMP_Text hpText;

    [Header("Shield UI (Player Only)")]
    public GameObject shieldRoot;
    public TMP_Text shieldText;

    [Header("Element Icon (Enemy Only)")]
    public Image elementIcon;

    [System.Serializable]
    public class ElementIconConfig
    {
        public ElementType element;
        public GameObject prefab;
    }

    public ElementIconConfig[] elementIcons;

    [Header("Billboard")]
    public bool faceCamera = true;

    private Camera mainCam;

    void Awake()
    {
        if (player == null)
            player = GetComponentInParent<PlayerStats>();

        if (enemy == null)
            enemy = GetComponentInParent<EnemyStats>();

        mainCam = Camera.main;

        Debug.Log($"WorldUnitHUD Awake - Player: {player != null}, Enemy: {enemy != null}");
    }

    void LateUpdate()
    {
        UpdateHPUI();
        UpdateElementIcon();
        UpdateShieldUI();
        if (faceCamera && mainCam != null)
        {
            Vector3 dir = transform.position - mainCam.transform.position;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    public void UpdateUI()
    {
        UpdateHPUI();
        UpdateElementIcon();
        UpdateShieldUI();
    }

    void UpdateHPUI()
    {
        if (hpFillImage == null && hpText == null)
            return;

        // ★ 매번 참조 다시 찾기
        if (player == null)
            player = GetComponentInParent<PlayerStats>();

        if (enemy == null)
            enemy = GetComponentInParent<EnemyStats>();

        float current = 0f;
        float max = 0f;

        if (player != null)
        {
            current = player.currentHP;
            max = player.maxHP;
        }
        else if (enemy != null)
        {
            current = enemy.currentHP;
            max = enemy.maxHP;
        }
        else
        {
            return;
        }

        if (max <= 0f) max = 1f;

        float ratio = Mathf.Clamp01(current / max);

        if (hpFillImage != null)
            hpFillImage.fillAmount = ratio;

        if (hpText != null)
        {
            int c = Mathf.CeilToInt(current);
            int m = Mathf.CeilToInt(max);
            hpText.text = string.Format("{0}/{1}", c, m);
        }
    }

    void UpdateElementIcon()
    {
        if (elementIcon == null)
            return;

        if (enemy == null)
        {
            elementIcon.enabled = false;
            return;
        }

        ElementType elem = enemy.elementType;

        Sprite sprite;
        Color color;
        if (TryGetIcon(elem, out sprite, out color))
        {
            elementIcon.enabled = true;
            elementIcon.sprite = sprite;
            elementIcon.color = color;
        }
        else
        {
            elementIcon.enabled = false;
        }
    }

    void UpdateShieldUI()
    {
        if (shieldRoot == null && shieldText == null)
            return;

        // 쉴드는 플레이어 기준으로만 표시
        if (player == null)
        {
            if (shieldRoot != null)
                shieldRoot.SetActive(false);
            if (shieldText != null)
                shieldText.gameObject.SetActive(false);
            return;
        }

        if (player.shield <= 0f)
        {
            if (shieldRoot != null)
                shieldRoot.SetActive(false);
            if (shieldText != null)
                shieldText.gameObject.SetActive(false);
        }
        else
        {
            if (shieldRoot != null)
                shieldRoot.SetActive(true);
            if (shieldText != null)
            {
                shieldText.gameObject.SetActive(true);
                int value = Mathf.CeilToInt(player.shield);
                shieldText.text = value.ToString();
            }
        }
    }

    bool TryGetIcon(ElementType elem, out Sprite sprite, out Color color)
    {
        sprite = null;
        color = Color.white;

        if (elementIcons == null)
            return false;

        foreach (var cfg in elementIcons)
        {
            if (cfg == null) continue;
            if (cfg.element != elem) continue;
            if (cfg.prefab == null) continue;

            SpriteRenderer sr = cfg.prefab.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) continue;

            sprite = sr.sprite;
            color = sr.color;
            return true;
        }

        return false;
    }
}