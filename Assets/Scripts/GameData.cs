using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    //Store all available champion, all champions must be assigned from the Editor to the Script GameObject
    public Champion[] championArray;
    //Store all available championTypes, all champions must be assigned from the Editor to the Script GameObject
    public ChampionType[] championTypesArray;

    //public DifficultySettings difficultySettings;
}
