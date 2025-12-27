using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 고정핀 시스템 - 특정 타일을 영구 고정
/// </summary>
public class PinSystem : MonoBehaviour
{
    private GridManager gridManager;

    // 고정된 타일 위치 (x, y 좌표)
    private HashSet<Vector2Int> pinnedPositions = new HashSet<Vector2Int>();

    // 고정핀 사용 가능 개수
    public int availablePins = 0;

    public void Initialize(GridManager grid)
    {
        gridManager = grid;
        pinnedPositions.Clear();
        availablePins = 0;
    }

    /// <summary>
    /// 고정핀 추가 (아이템으로 획득)
    /// </summary>
    public void AddPin(int count = 1)
    {
        availablePins += count;
        Debug.Log($"고정핀 획득: +{count} (현재 {availablePins}개)");
    }

    /// <summary>
    /// 타일 고정 (x, y 좌표)
    /// </summary>
    public bool PinTile(int x, int y)
    {
        if (availablePins <= 0)
        {
            Debug.Log("고정핀이 부족합니다!");
            return false;
        }

        Vector2Int pos = new Vector2Int(x, y);

        if (pinnedPositions.Contains(pos))
        {
            Debug.Log("이미 고정된 타일입니다.");
            return false;
        }

        pinnedPositions.Add(pos);
        availablePins--;

        Debug.Log($"타일 ({x}, {y}) 고정! 남은 핀: {availablePins}개");
        return true;
    }

    /// <summary>
    /// 타일 고정 해제
    /// </summary>
    public bool UnpinTile(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        if (!pinnedPositions.Contains(pos))
        {
            Debug.Log("고정되지 않은 타일입니다.");
            return false;
        }

        pinnedPositions.Remove(pos);
        availablePins++;

        Debug.Log($"타일 ({x}, {y}) 고정 해제! 남은 핀: {availablePins}개");
        return true;
    }

    /// <summary>
    /// 해당 위치가 고정되어 있는지 확인
    /// </summary>
    public bool IsPinned(int x, int y)
    {
        return pinnedPositions.Contains(new Vector2Int(x, y));
    }

    /// <summary>
    /// 모든 고정핀 해제
    /// </summary>
    public void ClearAllPins()
    {
        availablePins += pinnedPositions.Count;
        pinnedPositions.Clear();
        Debug.Log("모든 고정핀 해제");
    }

    /// <summary>
    /// 현재 고정된 타일 개수
    /// </summary>
    public int GetPinnedCount()
    {
        return pinnedPositions.Count;
    }

    /// <summary>
    /// 고정되지 않은 타일 위치 목록 반환
    /// </summary>
    public List<Vector2Int> GetUnpinnedPositions()
    {
        List<Vector2Int> unpinned = new List<Vector2Int>();

        if (gridManager == null) return unpinned;

        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!pinnedPositions.Contains(pos))
                {
                    unpinned.Add(pos);
                }
            }
        }

        return unpinned;
    }
}