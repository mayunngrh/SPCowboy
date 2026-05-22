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

    public ElementType enemyElement = ElementType.Metal;
    public ShotSetup currentShot = new ShotSetup();
    public ShootingMinigame shootingMinigame;

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
        Debug.Log("EndTurn called! isPlayerTurn: " + isPlayerTurn);
        if (!isPlayerTurn) return;
        isPlayerTurn = false;

        HandManager handManager = FindFirstObjectByType<HandManager>();
        Debug.Log("Starting DiscardAndShoot coroutine");
        StartCoroutine(DiscardAndShoot(handManager));
    }

    IEnumerator DiscardAndShoot(HandManager handManager)
    {
        Debug.Log("DiscardAndShoot started");
        yield return StartCoroutine(handManager.DiscardRemainingCards());
        Debug.Log("Cards discarded, waiting...");
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Opening shooting phase. shootingMinigame null? "
            + (shootingMinigame == null));
        shootingMinigame.OpenShootingPhase(currentShot);
        Debug.Log("OpenShootingPhase called!");
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

    // Called by ShootingMinigame when player stops the marker
    public void OnPlayerShoot(float markerPosition)
    {
        int totalDamage = 0;

        foreach (HitZone zone in currentShot.hitZones)
        {
            if (markerPosition >= zone.position &&
                markerPosition <= zone.position + zone.width)
            {
                totalDamage += zone.damage;

                // Check elemental bonus
                if (zone.bonusVsElement != ElementType.None &&
                    zone.bonusVsElement == enemyElement)
                {
                    totalDamage += zone.bonusDamage;
                    Debug.Log("Elemental bonus! +" + zone.bonusDamage);
                }

                Debug.Log("Hit zone: " + zone.element + " +" + zone.damage);
            }
        }

        Debug.Log("Shot dealt " + totalDamage + " damage!");

        if (totalDamage > 0)
            enemy.TakeDamage(totalDamage);

        currentShot.Reset();
        StartCoroutine(EnemyTurnRoutine());
    }

    public void ApplyCardEffect(CardData card)
    {
        switch (card.cardType)
        {
            case CardType.Basic:
            case CardType.Elemental:
                if (currentShot.CanAddZone())
                    currentShot.AddZone(card, shootingMinigame.barWidth);
                else
                    Debug.Log("No bullet slots left!");
                break;

            case CardType.Buff:
                currentShot.markerSpeed += card.markerSpeedModifier;
                currentShot.ApplyWidthModifier(card.zoneWidthModifier);
                break;

            case CardType.Utility:
                currentShot.maxBulletSlots += card.extraBulletSlots;
                GainEnergy(0);
                break;

            case CardType.Tradeoff:
                if (currentShot.CanAddZone())
                    currentShot.AddZone(card, shootingMinigame.barWidth);
                currentShot.markerSpeed += card.markerSpeedModifier;
                currentShot.ApplyWidthModifier(card.zoneWidthModifier);
                break;

            case CardType.Support:
                if (card.healAmount > 0)
                {
                    player.currentHP = Mathf.Min(
                        player.currentHP + card.healAmount, player.maxHP);
                    player.RefreshDisplay();
                }
                break;
        }

        shootingMinigame.RefreshZones(currentShot);
    }
}