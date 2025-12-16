using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PsbSpriteRebinder2
{
    [MenuItem("Tools/PSD/Rebind sprites (by current sprite's parent asset)")]
    private static void RebindByParentAsset()
    {
        var go = Selection.activeObject as GameObject;
        if (go == null)
        {
            Debug.LogError("Hierarchy(또는 Project의 Prefab)에서 루트 GameObject 하나를 선택하고 실행해.");
            return;
        }

        var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);
        int changed = 0;

        // 캐시: 같은 PSB/PSD는 한 번만 LoadAllAssets 하게
        var cache = new Dictionary<string, Dictionary<string, Sprite>>();

        foreach (var sr in renderers)
        {
            var cur = sr.sprite;
            if (cur == null) continue;

            // 이 스프라이트가 속한 부모 에셋 경로(PSB/PSD)
            var parentPath = AssetDatabase.GetAssetPath(cur);
            if (string.IsNullOrEmpty(parentPath)) continue;

            if (!cache.TryGetValue(parentPath, out var bestByName))
            {
                var sprites = AssetDatabase.LoadAllAssetsAtPath(parentPath).OfType<Sprite>().ToList();
                bestByName = sprites
                    .GroupBy(s => s.name)
                    .ToDictionary(g => g.Key,
                        g => g.OrderByDescending(s => s.rect.width * s.rect.height).First());
                cache[parentPath] = bestByName;

                Debug.Log($"[Rebind] 부모 에셋: {parentPath} (sprites: {sprites.Count})");
            }

            // 같은 이름이면 “Rect가 더 큰(=새로 reslice된)” 걸로 교체
            if (bestByName.TryGetValue(cur.name, out var best) && best != null && best != cur)
            {
                Undo.RecordObject(sr, "Rebind Sprite");
                sr.sprite = best;
                changed++;
            }
        }

        Debug.Log($"Rebind 완료: {changed}개 교체됨 (루트: {go.name}).");
    }
}
