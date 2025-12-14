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

    [Header("Binding")]
    [Tooltip("如果你的UI物体有 Tag=MainUI，会优先从这里找子物体；否则会从当前场景任意位置找。")]
    [SerializeField] private string mainUiTag = "MainUI";

    private static readonly HashSet<FragmentId> collected = new HashSet<FragmentId>();
    public int CollectedCount => collected.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        TryBindSlotsFromScene();
        RefreshAllIcons();
    }

    private Canvas flyCanvas;

    public Canvas GetOrCreateFlyCanvas()
    {
        if (flyCanvas != null) return flyCanvas;

        GameObject go = new GameObject("FlyCanvas");
        DontDestroyOnLoad(go);

        flyCanvas = go.AddComponent<Canvas>();
        flyCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        flyCanvas.sortingOrder = 9999;

        var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        return flyCanvas;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindSlotsFromScene();
        RefreshAllIcons();
    }

    private void TryBindSlotsFromScene()
    {
        // 1) 优先从 tag=MainUI 找
        GameObject mainUi = null;
        if (!string.IsNullOrEmpty(mainUiTag))
            mainUi = GameObject.FindGameObjectWithTag(mainUiTag);

        // 2) 找不到就退化：从场景中找 Image 名字匹配 FragmentId
        foreach (var slot in slots)
        {
            if (slot.icon != null) continue;

            if (mainUi != null)
            {
                Transform child = mainUi.transform.Find(slot.id.ToString());
                if (child != null)
                {
                    slot.icon = child.GetComponent<Image>();
                    if (slot.icon != null) continue;
                }
            }

            // fallback：全场景找一个同名 GameObject
            var go = GameObject.Find(slot.id.ToString());
            if (go != null)
            {
                slot.icon = go.GetComponent<Image>();
            }
        }
    }

    public bool IsCollected(FragmentId id) => collected.Contains(id);

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

    // ====== 给 FragmentEndPoint 用的两个接口 ======

    public RectTransform GetIconRect(FragmentId id)
    {
        foreach (var slot in slots)
        {
            if (slot.id == id && slot.icon != null)
                return slot.icon.rectTransform;
        }
        return null;
    }

    public Canvas GetFlyCanvas()
    {
        // 让飞行影子跟着 icon 所在 Canvas，避免“飞到世界坐标系 canvas”那种错位
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].icon == null) continue;
            var c = slots[i].icon.GetComponentInParent<Canvas>();
            if (c != null) return c;
        }

        // fallback：随便找一个 Canvas
        return FindObjectOfType<Canvas>();
    }
}
