using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image hpBarFill;
    public Image blockBarFill;
    public Image hpBarBackground;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpNumberText;

    [Header("Settings")]
    public Color fullHPColor = new Color(0.2f, 0.8f, 0.2f);
    public Color midHPColor = new Color(0.9f, 0.7f, 0.1f);
    public Color lowHPColor = new Color(0.9f, 0.2f, 0.1f);
    public Color blockColor = new Color(0f, 0.8f, 1f);

    private float maxHP;
    private float currentHP;
    private float maxBarWidth = 150f;

    void Awake()
    {
        StartCoroutine(InitBarWidth());
    }

    IEnumerator InitBarWidth()
    {
        yield return null; // wait one frame for layout
        if (hpBarBackground != null)
        {
            float w = hpBarBackground.rectTransform.sizeDelta.x;
            if (w > 0) maxBarWidth = w;
            Debug.Log("MaxBarWidth initialized: " + maxBarWidth);
        }

        if (blockBarFill != null)
            SetBarWidthDirect(blockBarFill, 0f);
    }

    public void Initialize(string name, int hp)
    {
        maxHP = hp;
        currentHP = hp;

        if (hpBarBackground != null)
        {
            float w = hpBarBackground.rectTransform.sizeDelta.x;
            if (w > 0) maxBarWidth = w;
        }

        if (nameText != null) nameText.text = name;
        UpdateHPNumber();
        SetBarWidthDirect(hpBarFill, maxBarWidth);

        if (blockBarFill != null)
            SetBarWidthDirect(blockBarFill, 0f);

        Debug.Log("HealthBar initialized. maxBarWidth: " + maxBarWidth);
    }

    public void UpdateHP(int newHP, int newBlock = 0)
    {
        Debug.Log("UpdateHP called! HP:" + newHP + " Block:" + newBlock);
        currentHP = newHP;
        float hpFill = Mathf.Clamp01(currentHP / maxHP);

        UpdateHPNumber();

        StopCoroutine("AnimateHPBar");
        StartCoroutine(AnimateHPBar(hpFill));

        StartCoroutine(UpdateBlockBarCoroutine(newBlock));
    }

    void UpdateHPNumber()
    {
        if (hpNumberText != null)
            hpNumberText.text = (int)currentHP + "/" + (int)maxHP;
    }

    IEnumerator UpdateBlockBarCoroutine(int block)
    {
        Debug.Log("UpdateBlockBarCoroutine START - block: " + block 
            + " blockBarFill null? " + (blockBarFill == null)
            + " maxBarWidth: " + maxBarWidth);

        if (blockBarFill == null) yield break;

        if (block <= 0)
        {
            yield return StartCoroutine(AnimateBarDirect(blockBarFill, 0f));
            yield break;
        }

        float targetWidth = Mathf.Min(block * 3f, maxBarWidth);
        blockBarFill.color = blockColor;

        Debug.Log("Setting block bar to width: " + targetWidth);

        // Skip animation, set directly first to test
        SetBarWidthDirect(blockBarFill, targetWidth);

        Debug.Log("Block bar width after set: " 
            + blockBarFill.rectTransform.sizeDelta.x);

        yield return StartCoroutine(AnimateBarDirect(blockBarFill, targetWidth));
    }

    IEnumerator AnimateHPBar(float targetFill)
    {
        float startWidth = hpBarFill.rectTransform.sizeDelta.x;
        float targetWidth = maxBarWidth * targetFill;
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / duration, 3f);
            SetBarWidthDirect(hpBarFill, Mathf.Lerp(startWidth, targetWidth, t));

            if (targetFill > 0.5f)
                hpBarFill.color = Color.Lerp(midHPColor, fullHPColor,
                    (targetFill - 0.5f) * 2f);
            else
                hpBarFill.color = Color.Lerp(lowHPColor, midHPColor,
                    targetFill * 2f);

            yield return null;
        }

        SetBarWidthDirect(hpBarFill, targetWidth);
    }

    IEnumerator AnimateBar(Image bar, float targetFill)
    {
        yield return StartCoroutine(
            AnimateBarDirect(bar, maxBarWidth * targetFill));
    }

    IEnumerator AnimateBarDirect(Image bar, float targetWidth)
    {
        float startWidth = bar.rectTransform.sizeDelta.x;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            SetBarWidthDirect(bar, Mathf.Lerp(startWidth, targetWidth, t));
            yield return null;
        }

        SetBarWidthDirect(bar, targetWidth);
        Debug.Log("Bar animated to width: " + targetWidth);
    }

    void SetBarWidth(Image bar, float fillAmount)
    {
        SetBarWidthDirect(bar, maxBarWidth * Mathf.Clamp01(fillAmount));
    }

    void SetBarWidthDirect(Image bar, float width)
    {
        if (bar == null) return;
        Vector2 size = bar.rectTransform.sizeDelta;
        size.x = Mathf.Max(0, width);
        bar.rectTransform.sizeDelta = size;
    }
}