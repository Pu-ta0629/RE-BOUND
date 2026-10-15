using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面の初期状態（位置・大きさ・回転・色・alpha・ON/OFF）を覚えておいて、入り直すたびに戻す。
/// Animatorが動かした値が、非表示にしても残ってしまう問題の対策。
/// </summary>
public class ViewSnapshot
{
    private class Entry
    {
        public Transform Target;
        public bool IsRoot;
        public bool RestoreActive;
        public bool Active;

        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;

        public RectTransform Rect;
        public Vector2 AnchorMin, AnchorMax, Pivot, AnchoredPosition, SizeDelta;

        public CanvasGroup Group;
        public float Alpha;
        public bool Interactable, BlocksRaycasts;

        public Graphic Graphic;
        public Color Color;
    }

    private readonly List<Entry> _entries = new();
    private readonly HashSet<Transform> _added = new();

    public bool IsTaken => _entries.Count > 0;
    public int Count => _entries.Count;

    public void Clear()
    {
        _entries.Clear();
        _added.Clear();
    }

    public void Take(Transform root)
    {
        Clear();
        Add(root, true);
    }

    public void Add(Transform root, bool restoreActive)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (!_added.Add(t)) continue;

            Entry e = new Entry
            {
                Target = t,
                IsRoot = t == root,
                RestoreActive = restoreActive,
                Active = t.gameObject.activeSelf,
                LocalPosition = t.localPosition,
                LocalRotation = t.localRotation,
                LocalScale = t.localScale
            };

            if (t is RectTransform rect)
            {
                e.Rect = rect;
                e.AnchorMin = rect.anchorMin;
                e.AnchorMax = rect.anchorMax;
                e.Pivot = rect.pivot;
                e.AnchoredPosition = rect.anchoredPosition;
                e.SizeDelta = rect.sizeDelta;
            }

            if (t.TryGetComponent(out CanvasGroup group))
            {
                e.Group = group;
                e.Alpha = group.alpha;
                e.Interactable = group.interactable;
                e.BlocksRaycasts = group.blocksRaycasts;
            }

            if (t.TryGetComponent(out Graphic graphic))
            {
                e.Graphic = graphic;
                e.Color = graphic.color;
            }

            _entries.Add(e);
        }
    }

    public void Restore()
    {
        foreach (Entry e in _entries)
        {
            if (e.Target == null) continue;

            if (e.RestoreActive && !e.IsRoot) e.Target.gameObject.SetActive(e.Active);

            if (e.Rect != null)
            {
                e.Rect.anchorMin = e.AnchorMin;
                e.Rect.anchorMax = e.AnchorMax;
                e.Rect.pivot = e.Pivot;
                e.Rect.anchoredPosition = e.AnchoredPosition;
                e.Rect.sizeDelta = e.SizeDelta;
            }
            else
            {
                e.Target.localPosition = e.LocalPosition;
            }

            e.Target.localRotation = e.LocalRotation;
            e.Target.localScale = e.LocalScale;

            if (e.Group != null)
            {
                e.Group.alpha = e.Alpha;
                e.Group.interactable = e.Interactable;
                e.Group.blocksRaycasts = e.BlocksRaycasts;
            }

            if (e.Graphic != null) e.Graphic.color = e.Color;
        }
    }
}