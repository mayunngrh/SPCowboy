using UnityEngine;

public enum CardType { Attack, Defense, Magic }

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    public CardType cardType;
    public int energyCost;
    [TextArea] public string description;
    public Sprite artwork;

    // Attack
    public int damageAmount;

    // Defense
    public int blockAmount;

    // Magic
    public int magicEffectValue;
}