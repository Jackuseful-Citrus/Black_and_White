using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInsultShooter : MonoBehaviour
{
    [Header("Base Settings")]
    public GameObject insultProjectilePrefab;
    public float minInterval = 4f;
    public float maxInterval = 9f;
    public float shootOffsetY = 0.5f;

    [Header("Insult Lines")]
    [TextArea]
    public List<string> insults = new List<string>()
    {
        "Idiot!",
        "Fool!",
        "Retard!",
        "Fuck you!",
    };

    [Header("Text Options")]
    [Tooltip("If true, replace any non-ASCII chars in insults with '?' to avoid missing glyph warnings.")]
    public bool sanitizeNonAscii = false;

    private Transform player;
    private bool isActive = true;

    private void Awake()
    {
        if (sanitizeNonAscii)
        {
            SanitizeInsults();
        }
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        StartCoroutine(TauntLoop());
    }

    private IEnumerator TauntLoop()
    {
        while (isActive && gameObject.activeInHierarchy)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (player != null && gameObject.activeInHierarchy)
            {
                ShootInsult();
            }
        }
    }

    private void ShootInsult()
    {
        if (insultProjectilePrefab == null || player == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * shootOffsetY;
        Vector2 dir = (player.position - spawnPos); // point to player

        GameObject go = Instantiate(insultProjectilePrefab, spawnPos, Quaternion.identity);
        var proj = go.GetComponent<InsultProjectile>();
        if (proj != null)
        {
            string text = insults.Count > 0
                ? insults[Random.Range(0, insults.Count)]
                : "...";

            proj.Init(dir, text, player, enableHoming: false);
        }
    }

    private void OnDisable()
    {
        isActive = false;
    }

    private void SanitizeInsults()
    {
        for (int i = 0; i < insults.Count; i++)
        {
            if (string.IsNullOrEmpty(insults[i])) continue;
            if (ContainsNonAscii(insults[i]))
            {
                insults[i] = ToAscii(insults[i]);
            }
        }
    }

    private string ToAscii(string s)
    {
        char[] buffer = new char[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            buffer[i] = c < 128 ? c : '?';
        }
        return new string(buffer);
    }

    private bool ContainsNonAscii(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] > 127) return true;
        }
        return false;
    }
}
