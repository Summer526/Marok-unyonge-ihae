using UnityEngine;

public class Tile : MonoBehaviour
{
    public ElementType elementType;
    public TileState state = TileState.Normal;
    public Vector2Int gridPos;

    public SpriteRenderer spriteRenderer;
    [Header("Highlight")]
    public GameObject highlightObj;
    private GridManager gridManager;
    private bool isDragging = false;

    public void Initialize(GridManager manager, Vector2Int pos, ElementType type)
    {
        gridManager = manager;
        gridPos = pos;
        SetElement(type);
        SetHighlighted(false);
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void SetElement(ElementType type)
    {
        elementType = type;
    }


    void OnMouseDown()
    {
        Debug.Log($"타일 OnMouseDown: {gridPos}");
        if (gridManager == null)
        {
            Debug.LogWarning("gridManager가 null!");
            return;
        }
        gridManager.BeginDrag(this);
    }

    void OnMouseUp()
    {
        Debug.Log($"타일 OnMouseUp: {gridPos}");
        if (gridManager == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        gridManager.EndDragAtPosition(mouseWorldPos);
    }
    public void SetHighlighted(bool on)
    {
        if (highlightObj != null)
        {
            highlightObj.SetActive(on);
        }
    }
}