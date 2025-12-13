using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingTwoPhaseUI : MonoBehaviour
{
    [Header("第一组：要滑入的\"字\"(TMP/Text 的 RectTransform)")]
    [SerializeField] private RectTransform titleText;

    [Header("第一组：进场直接显示的图片/其它元素（会在切换时一起隐藏）")]
    [SerializeField] private GameObject[] groupAStatic; // 把图片对象拖进来（也可拖多个）

    [Header("第二组：整体父物体（切换时显示）")]
    [SerializeField] private GameObject groupBRoot;

    [Header("滑入参数（anchoredPosition，像素）")]
    [SerializeField] private Vector2 fromPos = new Vector2(0, -1000);
    [SerializeField] private Vector2 toPos   = new Vector2(0, 20);
    [SerializeField] private float slideDuration = 1.2f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("滑入完成后停留多久再切换")]
    [SerializeField] private float holdSeconds = 3f;

    [Header("返回主页面")]
    [SerializeField] private string mainMenuScene = "Opening"; 
    [SerializeField] private bool enableReturnToMenu = true; 

    [Header("时间设置")]
    [SerializeField] private bool useUnscaledTime = true; 
    [SerializeField] private bool playOnStart = true;

    private Coroutine routine;
    private bool canReturn = false; 
    void Start()
    {
        if (playOnStart) Play();
    }

    void Update()
    {
        if (enableReturnToMenu && canReturn && Input.anyKeyDown)
        {
            ReturnToMainMenu();
        }
    }

    public void Play()
    {
        if (routine != null) StopCoroutine(routine);
        canReturn = false;
        routine = StartCoroutine(Sequence());
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(mainMenuScene);
    }

    private IEnumerator Sequence()
    {
        if (!titleText) yield break;

        // 初始：第二组隐藏
        if (groupBRoot) groupBRoot.SetActive(false);

        // 第一组图片/其它：直接显示
        if (groupAStatic != null)
        {
            foreach (var go in groupAStatic)
                if (go) go.SetActive(true);
        }

        // 第一组字：显示并放到起点
        titleText.gameObject.SetActive(true);
        titleText.anchoredPosition = fromPos;

        // 滑入字
        yield return Slide(titleText, fromPos, toPos, slideDuration);

        // 停留
        if (holdSeconds > 0f)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(holdSeconds);
            else yield return new WaitForSeconds(holdSeconds);
        }

        // 切换：第一组（字 + 图）一起隐藏
        titleText.gameObject.SetActive(false);
        if (groupAStatic != null)
        {
            foreach (var go in groupAStatic)
                if (go) go.SetActive(false);
        }

        // 第二组显示
        if (groupBRoot) groupBRoot.SetActive(true);

        // 第二组显示后，允许按键返回
        canReturn = true;

        routine = null;
    }

    private IEnumerator Slide(RectTransform rt, Vector2 start, Vector2 end, float duration)
    {
        if (duration <= 0f)
        {
            rt.anchoredPosition = end;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;

            float k = Mathf.Clamp01(t / duration);
            float e = ease != null ? ease.Evaluate(k) : k;

            rt.anchoredPosition = Vector2.LerpUnclamped(start, end, e);
            yield return null;
        }

        rt.anchoredPosition = end;
    }
}
