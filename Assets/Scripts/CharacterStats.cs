using UnityEngine;
using TMPro;

public class CharacterStats : MonoBehaviour
{
    public string characterName = "Character";
    public int maxHP = 50;
    public int currentHP;
    public int currentBlock = 0;

    public TextMeshProUGUI hpText;
    public HealthBar healthBar;

    void Start()
    {
        currentHP = maxHP;
        if (healthBar != null)
            healthBar.Initialize(characterName, maxHP);
        UpdateHPDisplay();
    }

    public void GainBlock(int amount)
    {
        currentBlock += amount;
        Debug.Log(characterName + " gained " + amount
            + " block. Total block: " + currentBlock);
        UpdateHPDisplay();
    }

    public void ResetBlock()
    {
        currentBlock = 0;
        UpdateHPDisplay();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);
        Debug.Log(characterName + " healed for " + amount
            + ". Current HP: " + currentHP + "/" + maxHP);
        UpdateHPDisplay();
    }

    public void TakeDamage(int amount)
    {
        int blockedAmount = 0;
        int actualDamage = amount;

        if (currentBlock > 0)
        {
            if (currentBlock >= amount)
            {
                blockedAmount = amount;
                currentBlock -= amount;
                actualDamage = 0;
                Debug.Log(characterName + " blocked ALL "
                    + amount + " damage! Block left: " + currentBlock);
            }
            else
            {
                blockedAmount = currentBlock;
                actualDamage = amount - currentBlock;
                currentBlock = 0;
                Debug.Log(characterName + " blocked " + blockedAmount
                    + " damage! " + actualDamage + " damage remaining.");
            }
        }

        currentHP -= actualDamage;
        currentHP = Mathf.Max(currentHP, 0);
        UpdateHPDisplay();

        if (currentHP <= 0)
            Debug.Log(characterName + " has died!");
    }

    void UpdateHPDisplay()
    {
        if (healthBar != null)
            healthBar.UpdateHP(currentHP, currentBlock);

        if (hpText == null) return;
        if (currentBlock > 0)
            hpText.text = characterName
                + "\nHP: " + currentHP + "/" + maxHP
                + "\nBlock: " + currentBlock;
        else
            hpText.text = characterName
                + "\nHP: " + currentHP + "/" + maxHP;
    }

    public void RefreshDisplay()
    {
        if (healthBar != null)
            healthBar.UpdateHP(currentHP, currentBlock);

        if (hpText == null) return;
        if (currentBlock > 0)
            hpText.text = characterName
                + "\nHP: " + currentHP + "/" + maxHP
                + "\nBlock: " + currentBlock;
        else
            hpText.text = characterName
                + "\nHP: " + currentHP + "/" + maxHP;
    }
}