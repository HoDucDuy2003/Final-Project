using UnityEngine;

[CreateAssetMenu(fileName = "DefaultChampion", menuName = "AutoChess/Champion",order = 1)]
public class Champion : ScriptableObject
{
    public GameObject prefab;

    public GameObject attackProjectile;

    public ChampionType type1;

    public ChampionType type2;

    public string ui_Name;

    public int cost;

    public Sprite display_cost;

    public float health;

    public float attackDamage;

    public float attackRange;
}
