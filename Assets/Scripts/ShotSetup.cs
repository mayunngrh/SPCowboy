using UnityEngine;
using System.Collections.Generic;

public class HitZone
{
    public ElementType element;
    public float position;
    public float width;
    public int damage;
    public int bonusDamage;
    public ElementType bonusVsElement;
}

public class ShotSetup
{
    public List<HitZone> hitZones = new List<HitZone>();
    public float markerSpeed = 1.0f;
    public int maxBulletSlots = 3;
    public int healAmount = 0;

    private float zoneWidthMod = 0f;

    public void Reset()
    {
        hitZones.Clear();
        markerSpeed = 1.0f;
        maxBulletSlots = 3;
        healAmount = 0;
        zoneWidthMod = 0f;
    }

    public bool CanAddZone()
    {
        return hitZones.Count < maxBulletSlots;
    }

    public void ApplyWidthModifier(float mod)
    {
        zoneWidthMod += mod;
    }

    public void AddZone(CardData card, float barWidth)
    {
        if (!CanAddZone()) return;

        float finalWidth = Mathf.Max(10f, card.hitZoneWidth + zoneWidthMod);
        float pos = GetSafePosition(finalWidth, barWidth);

        HitZone zone = new HitZone
        {
            element = card.element,
            position = pos,
            width = finalWidth,
            damage = card.hitZoneDamage,
            bonusDamage = card.elementalBonus,
            bonusVsElement = card.bonusVsElement
        };

        hitZones.Add(zone);
        Debug.Log("Added zone: " + card.element
            + " pos:" + pos
            + " width:" + finalWidth
            + " dmg:" + card.hitZoneDamage);
    }

    float GetSafePosition(float zoneWidth, float barWidth)
    {
        int attempts = 20;
        while (attempts-- > 0)
        {
            float pos = Random.Range(10f, barWidth - zoneWidth - 10f);
            bool overlaps = false;

            foreach (HitZone z in hitZones)
            {
                if (pos < z.position + z.width + 5f &&
                    pos + zoneWidth > z.position - 5f)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps) return pos;
        }

        return barWidth - zoneWidth - 10f;
    }
}