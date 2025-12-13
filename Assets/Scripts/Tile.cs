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
        if (gridManager == null) return;

        // 드래그 시작 타일 등록
        gridManager.BeginDrag(this);
    }

    void OnMouseUp()
    {
        if (gridManager == null) return;

        // 마우스가 떨어진 위치(월드 좌표)
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 드래그 종료 처리
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