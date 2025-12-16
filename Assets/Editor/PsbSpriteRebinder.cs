using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PsbSpriteRebinder
{
    [MenuItem("Tools/PSD/Rebind sprites from selected PSB (pick largest rect per name)")]
    private static void Rebind()
    {
        var selected = Selection.objects;
        if (selected == null || selected.Length < 2)
        {
            Debug.LogError("선택 순서: (1) PSB 에셋, (2) 씬/프리팹의 루트 GameObject 를 같이 선택해.");
            return;
        }

        // PSB asset path
        var psb = selected.FirstOrDefault(o => AssetDatabase.GetAssetPath(o).EndsWith(".psb") || AssetDatabase.GetAssetPath(o).EndsWith(".psd"));
        var go = selected.OfType<GameObject>().FirstOrDefault();

        if (psb == null || go == null)
        {
            Debug.LogError("PSB(.psb/.psd) 1개 + GameObject 1개를 같이 선택해.");
            return;
        }

        var path = AssetDatabase.GetAssetPath(psb);

        // Load all sprite sub-assets from the PSB
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToList();
        if (sprites.Count == 0)
        {
            Debug.LogError("선택한 PSB에서 Sprite 서브에셋을 못 찾음.");
            return;
        }

        // For each name, pick the sprite with the largest rect area (usually the newly resliced one)
        Dictionary<string, Sprite> bestByName = sprites
            .GroupBy(s => s.name)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(s => s.rect.width * s.rect.height).First()
            );

        var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);
        int changed = 0;

        foreach (var sr in renderers)
        {
            if (sr.sprite == null) continue;

            if (bestByName.TryGetValue(sr.sprite.name, out var best) && best != null && sr.sprite != best)
            {
                Undo.RecordObject(sr, "Rebind Sprite");
                sr.sprite = best;
                changed++;
            }
        }

        EditorUtility.SetDirty(go);
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);

        Debug.Log($"Rebind 완료: {changed}개 SpriteRenderer 교체됨. (PSB: {System.IO.Path.GetFileName(path)})");
    }
}
