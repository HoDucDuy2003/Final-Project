using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionShop : MonoBehaviour
{
    public UIController uiController;
    public GameData gameData;
    public GamePlayController gamePlayController;

    private Champion[] availableChampionArray;
    // Start is called before the first frame update
    void Start()
    {
        RefreshShop(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RefreshShop(bool isFree)
    {
        if (gamePlayController.currentGold < 2 && isFree == false)
            return;

        availableChampionArray = new Champion[5];

        for(int i = 0; i < availableChampionArray.Length; i++)
        {
            Champion champion = GetRandomChampionInfo();

            availableChampionArray[i] = champion;

            uiController.LoadShop(champion,i);

            uiController.ShowShopItems();
        }
        if (!isFree)
        {
            gamePlayController.currentGold -= 2;
        }

        uiController.UpdateUI();
    }
    public void OnChampionFrameClicked(int index)
    {
        bool isSuccess = gamePlayController.BuyChampionFromShop(availableChampionArray[index]);

        if (isSuccess)
            uiController.HideChampionFrame(index);
    }
    public Champion GetRandomChampionInfo()
    {
        int rand = Random.Range(0, gameData.championArray.Length);

        return gameData.championArray[rand];
    }
    public void BuyLvl()
    {
        gamePlayController.BuyLvl();
    }
}
