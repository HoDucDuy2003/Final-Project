using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefauultChampionType", menuName = "AutoChess/ChampionType", order = 2)]
public class ChampionType : ScriptableObject
{
    public string displayName = "name";

    public Sprite icon;

    public ChampionBonus championBonus;

}
