using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultChampion", menuName = "AutoChess/Champion",order = 1)]
public class Champion : ScriptableObject
{
    public GameObject prefab;

    public GameObject attackProjectile;
    public string ui_Name;

    public ChampionType type1;

    public ChampionType type2;

    public int cost;

    public Sprite display_cost;

    public float health;

    public float attackDamage;

    public float attackRange;
}
