using System;
using System.Collections.Generic;
using StorageFilterIcons.Options;
using UnityEngine;

namespace StorageFilterIcons.Components;

// Draws the icons of the filtered (accepted) resources over the building sprite.
// Added to every completed building that has a TreeFilterable + Storage (see Patches/BuildingPatches).
public sealed class FilterIconOverlay : MonoBehaviour
{
    public const int MaxIcons = 8;
    private const float HoverScaleFactor = 1.4f;
    private const float IconFill = 0.85f;
    private const float MaxIconSize = 1f;

    // Fine adjustments in tiles / relative to the cell size.
    private const float OffsetX = 0f;

    // Extra horizontal shift as a fraction of the icon size (negative = left); scales with how large the icon is drawn.
    private const float IconOffsetXRatio = 0f;
    private const float MoreTextOffsetY = 0.18f;
    private const float MoreTextScale = 0.17f;

    private static readonly List<FilterIconOverlay> All = new List<FilterIconOverlay>();
    private static Material spriteMaterial;
    private static Font textFont;

    private TreeFilterable filterable;
    private Storage storage;
    private Building building;
    private string prefabId;
    private GameObject root;
    private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private TextMesh moreText;
    private List<FilterIcon> icons = new List<FilterIcon>();
    private bool hovered;

    internal bool IsActive => root != null && root.activeSelf;

    internal IReadOnlyList<FilterIcon> Icons => icons;

    internal int HiddenCount => Math.Max(0, icons.Count - MaxIcons);

    internal bool IsEnabledByOptions => ModOptions.Instance.IsBuildingEnabled(prefabId);

    public static void RefreshAll()
    {
        foreach (var overlay in All.ToArray())
        {
            if (overlay != null)
            {
                overlay.Refresh();
            }
        }
    }

    private void Start()
    {
        filterable = GetComponent<TreeFilterable>();
        storage = GetComponent<Storage>();
        building = GetComponent<Building>();
        if (filterable == null || storage == null || building == null)
        {
            Destroy(this);
            return;
        }

        prefabId = GetComponent<KPrefabID>().PrefabTag.Name;
        root = new GameObject("StorageFilterIcons");
        root.transform.SetParent(transform, false);
        root.layer = gameObject.layer;

        All.Add(this);
        filterable.OnFilterChanged += OnFilterChanged;
        Refresh();
    }

    private void OnDestroy()
    {
        All.Remove(this);
        if (filterable != null)
        {
            filterable.OnFilterChanged -= OnFilterChanged;
        }

        if (root != null)
        {
            Destroy(root);
        }
    }

    private void Update()
    {
        if (root == null)
        {
            return;
        }

        var nowHovered = HoverTracker.Current == gameObject;
        if (nowHovered != hovered)
        {
            hovered = nowHovered;
            ApplyHover();
        }
    }

    private void OnFilterChanged(HashSet<Tag> tags)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (root == null)
        {
            return;
        }

        icons = ModOptions.Instance.IsBuildingEnabled(prefabId)
            ? FilterIconSource.GetIcons(filterable, storage)
            : new List<FilterIcon>();
        Rebuild();
        ApplyHover();
    }

    private void Rebuild()
    {
        var shown = Math.Min(icons.Count, MaxIcons);
        var hidden = HiddenCount;
        var cellCount = shown + (hidden > 0 ? 1 : 0);

        if (cellCount == 0)
        {
            root.SetActive(false);
            return;
        }

        GetFootprint(out var center, out var width, out var height);
        Layout(cellCount, width, height, out var cols, out var rows, out var cellSize);

        var position = new Vector3(center.x + OffsetX, center.y, Grid.GetLayerZ(Grid.SceneLayer.BuildingFront));
        root.transform.SetPosition(position);
        root.transform.localScale = Vector3.one;

        for (var i = 0; i < cellCount; i++)
        {
            var row = i / cols;
            var col = i % cols;
            var inRow = Math.Min(cols, cellCount - row * cols);
            var x = (col - (inRow - 1) / 2f) * cellSize;
            var y = ((rows - 1) / 2f - row) * cellSize;
            var local = new Vector3(x, y, 0f);

            if (i < shown)
            {
                var renderer = GetRenderer(i);
                renderer.sprite = icons[i].Sprite;
                var spriteBounds = icons[i].Sprite.bounds;
                var spriteSize = spriteBounds.size;
                var scale = cellSize * IconFill / Math.Max(spriteSize.x, spriteSize.y);
                // Cancel an off-center sprite pivot so the visible image is centered in its cell.
                var pivotFix = new Vector3(-spriteBounds.center.x * scale + IconOffsetXRatio * cellSize * IconFill, -spriteBounds.center.y * scale, 0f);
                renderer.gameObject.transform.localPosition = local + pivotFix;
                renderer.gameObject.transform.localScale = new Vector3(scale, scale, 1f);
                renderer.gameObject.SetActive(true);
            }
            else
            {
                var text = GetMoreText();
                text.text = "+" + hidden;
                text.gameObject.transform.localPosition = local + new Vector3(0f, cellSize * MoreTextOffsetY, 0f);
                var s = cellSize * IconFill * MoreTextScale;
                text.gameObject.transform.localScale = new Vector3(s, s, 1f);
                text.gameObject.SetActive(true);
            }
        }

        for (var i = shown; i < renderers.Count; i++)
        {
            renderers[i].gameObject.SetActive(false);
        }

        if (hidden == 0 && moreText != null)
        {
            moreText.gameObject.SetActive(false);
        }

        root.SetActive(true);
    }

    private void ApplyHover()
    {
        if (root == null)
        {
            return;
        }

        var options = ModOptions.Instance;
        var visible = icons.Count > 0 && !(hovered && options.HoverScale == HoverScaleMode.Hide);
        if (root.activeSelf != visible)
        {
            root.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        var enlarge = hovered && options.HoverScale == HoverScaleMode.Enlarge;
        root.transform.localScale = enlarge ? new Vector3(HoverScaleFactor, HoverScaleFactor, 1f) : Vector3.one;

        var alpha = hovered && options.HoverOpacity == HoverOpacityMode.Opaque ? 1f : options.Opacity;
        for (var i = 0; i < renderers.Count && i < icons.Count; i++)
        {
            var c = icons[i].Color;
            renderers[i].color = new Color(c.r, c.g, c.b, alpha);
        }

        if (moreText != null)
        {
            moreText.color = new Color(1f, 1f, 1f, alpha);
        }
    }

    private SpriteRenderer GetRenderer(int index)
    {
        while (renderers.Count <= index)
        {
            var go = new GameObject("Icon" + renderers.Count);
            go.transform.SetParent(root.transform, false);
            go.layer = root.layer;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = GetSpriteMaterial() ?? renderer.sharedMaterial;
            renderer.sortingOrder = 1;
            renderers.Add(renderer);
        }

        return renderers[index];
    }

    private TextMesh GetMoreText()
    {
        if (moreText == null)
        {
            var go = new GameObject("More");
            go.transform.SetParent(root.transform, false);
            go.layer = root.layer;
            moreText = go.AddComponent<TextMesh>();
            moreText.anchor = TextAnchor.MiddleCenter;
            moreText.alignment = TextAlignment.Center;
            moreText.fontSize = 64;
            moreText.characterSize = 1f;
            var font = GetFont();
            if (font != null)
            {
                moreText.font = font;
                var meshRenderer = go.GetComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = font.material;
                meshRenderer.sortingOrder = 1;
            }
        }

        return moreText;
    }

    private void GetFootprint(out Vector2 center, out float width, out float height)
    {
        var cells = building.PlacementCells;
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;
        foreach (var cell in cells)
        {
            Grid.CellToXY(cell, out var x, out var y);
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        center = new Vector2((minX + maxX) / 2f + 0.5f, (minY + maxY) / 2f + 0.5f);
        width = maxX - minX + 1;
        height = maxY - minY + 1;
    }

    // Picks the grid (cols x rows) that makes the icons as large as possible inside the footprint.
    private static void Layout(int count, float width, float height, out int cols, out int rows, out float cellSize)
    {
        cols = 1;
        rows = count;
        cellSize = 0f;
        for (var c = 1; c <= count; c++)
        {
            var r = (count + c - 1) / c;
            var size = Math.Min(width / c, height / r);
            if (size > cellSize + 1e-4f)
            {
                cellSize = size;
                cols = c;
                rows = r;
            }
        }

        cellSize = Math.Min(cellSize, MaxIconSize);
    }

    private static Material GetSpriteMaterial()
    {
        if (spriteMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                spriteMaterial = new Material(shader);
            }
        }

        return spriteMaterial;
    }

    private static Font GetFont()
    {
        if (textFont == null)
        {
            try
            {
                textFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (Exception)
            {
                // Older Unity versions ship the built-in font under a different name.
            }

            if (textFont == null)
            {
                try
                {
                    textFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch (Exception)
                {
                    // No built-in font; the "+N" label is then simply not shown.
                }
            }
        }

        return textFont;
    }
}
