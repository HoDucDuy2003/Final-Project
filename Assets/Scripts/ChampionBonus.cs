using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChampionBonusType { Damage,Defense,Stun,Heal};
public enum BonusTarget { Self,Enemies };


[System.Serializable]
public class ChampionBonus
{
    // How many champion needed to activate this bonus
    public int championCount = 0;

    public ChampionBonusType championBonusType;
    public BonusTarget bonusTarget;

    public float bonusValue = 0;
    public float duration = 0;

    public GameObject effectPrefab;

    // Calculates bonuses of a champion when attacking
    public float ApplyOnAttack(ChampionController champion, ChampionController championTarget)
    {
        float bonusDamage = 0;
        bool addEffect = false;

        switch (championBonusType)
        {
            case ChampionBonusType.Damage:
                bonusDamage += bonusValue;
                break;
            case ChampionBonusType.Stun:

                int rand = Random.Range(0, 100);
                if (rand < bonusValue)
                {
                    championTarget.OnGotStun(duration);
                    addEffect = true;
                }
                break;
            case ChampionBonusType.Heal:
                champion.OnGotHeal(bonusValue);
                addEffect = true;
                break;
            default:
                break;
        }
        if (addEffect)
        {
            if (bonusTarget == BonusTarget.Self)
            {
                champion.AddEffect(effectPrefab, duration);
            }
            else if (bonusTarget == BonusTarget.Enemies)
            {
                championTarget.AddEffect(effectPrefab, duration);
            }
        }
        return bonusDamage;
    }
    public float ApplyOnGotHit(ChampionController champiom,float damage)
    {
        switch (championBonusType)
        {
            case ChampionBonusType.Defense:
                damage = ((100 - bonusValue) / 100) * damage;
                break;
            default:
                break;
        }
        return damage;
    }
 }
    