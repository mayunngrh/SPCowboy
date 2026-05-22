using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AnimationManager : MonoBehaviour
{
    [Header("References")]
    public RectTransform playerSprite;
    public RectTransform enemySprite;

    [Header("Animation Settings")]
    public float strikeDistance = 150f;
    public float strikeDuration = 0.15f;
    public float returnDuration = 0.2f;
    public float hitFlashDuration = 0.3f;

    private Vector3 playerOriginalPos;
    private Vector3 enemyOriginalPos;

    private Image playerImage;
    private Image enemyImage;

    void Start()
    {
        playerOriginalPos = playerSprite.localPosition;
        enemyOriginalPos = enemySprite.localPosition;

        playerImage = playerSprite.GetComponent<Image>();
        enemyImage = enemySprite.GetComponent<Image>();
    }

    // Player attacks enemy
    public IEnumerator PlayAttackAnimation(CardData card, CharacterStats target, int damage)
    {
        CombatManager combatManager = FindFirstObjectByType<CombatManager>();
        bool isPlayerAttacking = (target == combatManager.enemy);

        if (isPlayerAttacking)
        {
            // Player lunges right
            yield return StartCoroutine(MoveSprite(playerSprite,
                playerOriginalPos + Vector3.right * strikeDistance, strikeDuration));

            // Enemy flashes red + damage text
            StartCoroutine(FlashSpriteRed(enemyImage));
            ShowDamageText(enemySprite, "-" + damage, Color.red);

            // Player returns
            yield return StartCoroutine(MoveSprite(playerSprite,
                playerOriginalPos, returnDuration));
        }
        else
        {
            // Magic or other — just flash enemy
            StartCoroutine(FlashSpriteRed(enemyImage));
            ShowDamageText(enemySprite, "-" + damage, Color.magenta);
            yield return new WaitForSeconds(0.4f);
        }
    }

    // Enemy attacks player
    public IEnumerator PlayEnemyAttackAnimation(int damage, int actualDamage, int blockedAmount)
    {
        
    // Enemy lunges LEFT
    yield return StartCoroutine(MoveSprite(enemySprite,
        enemyOriginalPos + Vector3.left * strikeDistance, strikeDuration));

    if (blockedAmount > 0 && actualDamage == 0)
    {
        // Fully blocked — show shield flash + blocked text
        StartCoroutine(FlashSpriteColor(playerImage, Color.yellow, hitFlashDuration));
        ShowDamageText(playerSprite, "BLOCKED!", Color.yellow);
    }
    else if (blockedAmount > 0 && actualDamage > 0)
    {
        // Partially blocked — show both
        StartCoroutine(FlashSpriteColor(playerImage, Color.yellow, hitFlashDuration));
        ShowDamageText(playerSprite, "-" + actualDamage + " (blocked " 
            + blockedAmount + ")", Color.yellow);
    }
    else
    {
        // No block — full damage
        StartCoroutine(FlashSpriteRed(playerImage));
        ShowDamageText(playerSprite, "-" + damage, Color.red);
    }

    // Enemy returns
    yield return StartCoroutine(MoveSprite(enemySprite,
        enemyOriginalPos, returnDuration));
}

    // Defense animation
    public IEnumerator PlayDefenseAnimation(int blockAmount)
    {
        yield return StartCoroutine(FlashSpriteColor(playerImage, Color.cyan, 0.3f));
        ShowDamageText(playerSprite, "+" + blockAmount + " Block", Color.cyan);
    }

    IEnumerator FlashSpriteRed(Image sprite)
    {
        yield return StartCoroutine(FlashSpriteColor(sprite, Color.red, hitFlashDuration));
    }

    IEnumerator FlashSpriteColor(Image sprite, Color flashColor, float duration)
    {
        sprite.color = flashColor;
        yield return new WaitForSeconds(duration);
        sprite.color = Color.white;
    }

    IEnumerator MoveSprite(RectTransform sprite, Vector3 target, float duration)
    {
        Vector3 start = sprite.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sprite.localPosition = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        sprite.localPosition = target;
    }

    void ShowDamageText(RectTransform target, string text, Color color)
    {
        GameObject dmgObj = new GameObject("DamageText");
        dmgObj.transform.SetParent(target, false);

        TextMeshProUGUI tmp = dmgObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rt = dmgObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 60);
        rt.localPosition = Vector3.up * 50f;

        StartCoroutine(FloatAndFade(rt, tmp));
    }

    IEnumerator FloatAndFade(RectTransform rt, TextMeshProUGUI tmp)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = rt.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.localPosition = startPos + Vector3.up * (50f * t);
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1f - t);
            yield return null;
        }

        Destroy(rt.gameObject);
    }
}