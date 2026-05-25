using UnityEngine;
using System.Collections;

public class WorldCharacters : MonoBehaviour
{
    [Header("World Sprites")]
    public SpriteRenderer playerWorldSprite;
    public SpriteRenderer enemyWorldSprite;

    private Vector3 playerOrigin;
    private Vector3 enemyOrigin;

    void Start()
    {
        if (playerWorldSprite)
            playerOrigin = playerWorldSprite.transform.position;
        if (enemyWorldSprite)
            enemyOrigin = enemyWorldSprite.transform.position;
    }

    public void MovePlayerForward(float distance)
    {
        StartCoroutine(MoveSprite(
            playerWorldSprite.transform,
            playerOrigin + Vector3.right * distance,
            0.15f));
    }

    public void MovePlayerBack()
    {
        StartCoroutine(MoveSprite(
            playerWorldSprite.transform,
            playerOrigin,
            0.2f));
    }

    public void MoveEnemyForward(float distance)
    {
        StartCoroutine(MoveSprite(
            enemyWorldSprite.transform,
            enemyOrigin + Vector3.left * distance,
            0.15f));
    }

    public void MoveEnemyBack()
    {
        StartCoroutine(MoveSprite(
            enemyWorldSprite.transform,
            enemyOrigin,
            0.2f));
    }

    public void FlashPlayerRed()
    {
        StartCoroutine(FlashSprite(playerWorldSprite, Color.red, 0.3f));
    }

    public void FlashEnemyRed()
    {
        StartCoroutine(FlashSprite(enemyWorldSprite, Color.red, 0.3f));
    }

    public void FlashPlayerCyan()
    {
        StartCoroutine(FlashSprite(playerWorldSprite, Color.cyan, 0.3f));
    }

    IEnumerator MoveSprite(Transform sprite, Vector3 target, float duration)
    {
        Vector3 start = sprite.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            sprite.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        sprite.position = target;
    }

    IEnumerator FlashSprite(SpriteRenderer sprite, Color flashColor, float duration)
    {
        sprite.color = flashColor;
        yield return new WaitForSeconds(duration);
        sprite.color = Color.white;
    }
} 