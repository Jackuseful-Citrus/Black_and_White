using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FragmentCollectionManager : MonoBehaviour
{
    public static FragmentCollectionManager Instance { get; private set; }

    [System.Serializable]
    public class FragmentSlot
    {
        public FragmentId id;
        public Image icon;
    }

    [Header("Slots")]
    public List<FragmentSlot> slots = new List<FragmentSlot>();

    [Header("Alpha")]
    [Range(0f, 1f)] public float dimAlpha = 0.25f;
    [Range(0f, 1f)] public float litAlpha = 1.0f;

    private static readonly HashSet<FragmentId> collected = new HashSet<FragmentId>();
    public int CollectedCount => collected.Count;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;

        TryBindSlotsFromScene();
        RefreshAllIcons();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindSlotsFromScene();
        RefreshAllIcons();
    }

    /// <summary>Ensure slot icons are bound after scene changes.</summary>
    private void TryBindSlotsFromScene()
    {
        var mainUi = GameObject.FindGameObjectWithTag("MainUI");
        if (mainUi == null) return;

        foreach (var slot in slots)
        {
            if (slot.icon != null) continue;

            Transform child = mainUi.transform.Find(slot.id.ToString());
            if (child != null)
            {
                slot.icon = child.GetComponent<Image>();
            }
        }
    }

    public bool IsCollected(FragmentId id)
    {
        return collected.Contains(id);
    }

    public void MarkCollected(FragmentId id)
    {
        if (!collected.Add(id))
            return;

        RefreshIcon(id);
    }

    public void RefreshAllIcons()
    {
        foreach (var slot in slots)
        {
            if (slot.icon == null) continue;

            bool has = collected.Contains(slot.id);
            ApplyAlpha(slot.icon, has ? litAlpha : dimAlpha);
        }
    }

    private void RefreshIcon(FragmentId id)
    {
        foreach (var slot in slots)
        {
            if (slot.id != id || slot.icon == null) continue;

            bool has = collected.Contains(id);
            ApplyAlpha(slot.icon, has ? litAlpha : dimAlpha);
            break;
        }
    }

    private void ApplyAlpha(Image img, float alpha)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
