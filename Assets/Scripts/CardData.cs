using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    public CardType cardType;
    public int energyCost;
    public string description;
    public Sprite artwork;

    [Header("Hit Zone (Basic / Elemental)")]
    public int hitZoneDamage;
    public float hitZoneWidth = 50f;
    public ElementType element;
    public int elementalBonus;
    public ElementType bonusVsElement;

    [Header("Buff / Utility Effects")]
    public float markerSpeedModifier;  // + = faster marker, - = slower
    public float zoneWidthModifier;    // + = wider hit zones
    public int extraBulletSlots;
    public int healAmount;

    [Header("Tradeoff")]
    public bool isTradeoff;
    public string tradeoffDescription;
}
