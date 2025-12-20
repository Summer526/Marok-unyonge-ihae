using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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

        // Collider2D 확인 및 추가
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
            Debug.Log($"타일 ({pos.x},{pos.y})에 BoxCollider2D 추가됨");
        }

        // Collider 크기 확인
        Debug.Log($"타일 ({pos.x},{pos.y}) Collider 크기: {collider.size}");

        // 너무 작으면 크기 조정
        if (collider.size.x < 0.5f || collider.size.y < 0.5f)
        {
            collider.size = new Vector2(1f, 1f);
            Debug.Log($"타일 ({pos.x},{pos.y}) Collider 크기 조정됨: 1x1");
        }
    }

    public void SetElement(ElementType type)
    {
        elementType = type;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"타일 OnMouseDown 시작: {gridPos}");

        if (gridManager == null)
        {
            Debug.LogError("gridManager가 null!");
            return;
        }

        Debug.Log($"BeginDrag 호출: {gridPos}");
        gridManager.BeginDrag(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"타일 OnMouseUp 시작: {gridPos}");

        if (gridManager == null)
        {
            Debug.LogError("gridManager가 null!");
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Debug.Log($"EndDragAtPosition 호출: {gridPos} -> {mouseWorldPos}");
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