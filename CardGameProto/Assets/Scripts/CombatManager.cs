using UnityEngine;
using TMPro;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    public CharacterStats player;
    public CharacterStats enemy;

    public int playerAttackDamage = 10;
    public int enemyAttackDamage = 7;

    public bool isPlayerTurn = true;

    public int energyPerTurn = 3;
    public int currentEnergy = 0;
    public int maxEnergy = 12;

    public TextMeshProUGUI energyText;

    void Start()
    {
        GainEnergy(energyPerTurn);
    }

    public void GainEnergy(int amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        UpdateEnergyDisplay();
    }

    public bool SpendEnergy(int amount)
    {
        if (currentEnergy < amount)
        {
            Debug.Log("Not enough energy!");
            return false;
        }
        currentEnergy -= amount;
        UpdateEnergyDisplay();
        return true;
    }

    void UpdateEnergyDisplay()
    {
        if (energyText != null)
            energyText.text = "Energy: " + currentEnergy + "/" + maxEnergy;
    }

    public void EndTurn()
    {
        if (!isPlayerTurn) return;

        Debug.Log("Player ends turn.");
        isPlayerTurn = false;

        HandManager handManager = FindFirstObjectByType<HandManager>();
        StartCoroutine(DiscardAndEnemyAttack(handManager));
    }

    IEnumerator DiscardAndEnemyAttack(HandManager handManager)
    {
        // Discard remaining cards
        yield return StartCoroutine(handManager.DiscardRemainingCards());

        yield return new WaitForSeconds(0.5f);

        // Enemy attacks
        yield return StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        // Small delay before enemy attacks
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(EnemyAttackRoutine());
    }

    IEnumerator EnemyAttackRoutine()
    {
        AnimationManager animManager = FindFirstObjectByType<AnimationManager>();

        int damage = enemyAttackDamage;
        int blockedAmount = 0;
        int actualDamage = damage;

        // Calculate how much block absorbs BEFORE animation
        if (player.currentBlock > 0)
        {
            if (player.currentBlock >= damage)
            {
                blockedAmount = damage;
                actualDamage = 0;
            }
            else
            {
                blockedAmount = player.currentBlock;
                actualDamage = damage - player.currentBlock;
            }
        }

        Debug.Log("Enemy attacks! Damage: " + damage 
            + " Blocked: " + blockedAmount 
            + " Actual: " + actualDamage);

        // Play animation showing correct result
        yield return StartCoroutine(
            animManager.PlayEnemyAttackAnimation(damage, actualDamage, blockedAmount));

        // Apply damage AFTER animation
        player.TakeDamage(damage);

        // NOW start player turn
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        player.ResetBlock();
        GainEnergy(energyPerTurn);

        HandManager handManager = FindFirstObjectByType<HandManager>();
        DeckManager deckManager = FindFirstObjectByType<DeckManager>();
        deckManager.DiscardHand();
        deckManager.DrawHand();
        handManager.DrawHandWithAnimation();
        
        Debug.Log("New player turn! Energy: " + currentEnergy);
    }

    public void PlayerAttack()
    {
        if (!isPlayerTurn) return;
        Debug.Log("Player attacks for " + playerAttackDamage);
        enemy.TakeDamage(playerAttackDamage);
        isPlayerTurn = false;
        StartCoroutine(EnemyTurnRoutine());
    }
}