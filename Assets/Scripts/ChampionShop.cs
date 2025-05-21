using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionShop : MonoBehaviour
{
    public UIController uiController;
    public GameData gameData;
    public GamePlayController gamePlayController;

    private Champion[] availableChampionArray;
    private Dictionary<Champion, int> championPool;

    private float[,] tierProbabilities = new float[9, 5] {
        //Tier 1, Tier 2, Tier 3, Tier 4, Tier 5
        { 1.00f, 0.00f, 0.00f, 0.00f, 0.00f }, // Level 1
        { 0.75f, 0.25f, 0.00f, 0.00f, 0.00f }, // Level 2
        { 0.70f, 0.25f, 0.05f, 0.00f, 0.00f }, // Level 3
        { 0.50f, 0.35f, 0.13f, 0.02f, 0.00f }, // Level 4
        { 0.33f, 0.35f, 0.25f, 0.07f, 0.00f }, // Level 5
        { 0.20f, 0.30f, 0.33f, 0.15f, 0.02f }, // Level 6
        { 0.15f, 0.20f, 0.35f, 0.25f, 0.05f }, // Level 7
        { 0.10f, 0.15f, 0.30f, 0.30f, 0.15f }, // Level 8
        { 0.05f, 0.10f, 0.20f, 0.35f, 0.30f }  // Level 9
    };

    private readonly int[] maxCopiesPerTier = { 30, 20, 15, 10, 9 };
    void Start()
    {
        //InitializeChampionPool();
        RefreshShop(true);
    }
    public Champion[] GenerateShop(int playerLevel)
    {
        Champion[] shopSlots = new Champion[5];
        for (int i = 0; i < 5; i++)
        {
            shopSlots[i] = GetRandomChampionInfo();     
        }
        return shopSlots;
    }
    public void RefreshShop(bool isFree)
    {
        if (gamePlayController.currentGold < 2 && isFree == false)
            return;

        availableChampionArray = GenerateShop(gamePlayController.currentChampionLimit);

        for (int i = 0; i < availableChampionArray.Length; i++)
        {
            Champion champion = availableChampionArray[i];
            if (champion != null)
            {
                uiController.LoadShop(champion, i);
                uiController.ShowChampionFrame(i);
            }
        }
       

        if (!isFree)
        {
            gamePlayController.currentGold -= 2;
        }

        uiController.UpdateUI();
    }
    public void OnChampionFrameClicked(int index)
    {
        Champion selectedChampion = BuyChampion(index, availableChampionArray);
        if (selectedChampion != null)
        {
            bool isSuccess = gamePlayController.BuyChampionFromShop(selectedChampion);
            if (isSuccess)
                uiController.HideChampionFrame(index);
               
        }

    }
    public Champion BuyChampion(int slotIndex, Champion[] shopSlots)
    {
        if (slotIndex >= 0 && slotIndex < shopSlots.Length && shopSlots[slotIndex] != null)
        {
            return shopSlots[slotIndex]; 
        }
        return null; 
    }
    public Champion GetRandomChampionInfo()
    {
        int playerLevel = gamePlayController.currentChampionLimit;

        float[] probabilities = new float[5];
        for (int i = 0; i < 5; i++)
        {
            probabilities[i] = tierProbabilities[playerLevel - 1, i];
        }
        int selectedTier = GetRandomTier(probabilities);

        List<Champion> championsInTier = new List<Champion>();
        foreach (Champion champion in gameData.championArray)
        {
            if (champion.cost == selectedTier + 1 ) //&& championPool.ContainsKey(champion) && championPool[champion] > 0)
            {
                championsInTier.Add(champion);
            }
        }
        //select random champion from the list of champions in the selected tier
        if (championsInTier.Count > 0)
        {
            int randIndex = Random.Range(0, championsInTier.Count);
            return championsInTier[randIndex];
        }

        int rand = Random.Range(0, gameData.championArray.Length);

        return gameData.championArray[rand];
    }

    private int GetRandomTier(float[] probabilities)
    {
        float randomValue = Random.Range(0f, 1f);
        float cumulative = 0f;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (randomValue <= cumulative)
            {
                return i;
            }
        }

        // Nếu không chọn được tier nào, trả về tier cuối cùng
        return probabilities.Length - 1;
    }
    private void InitializeChampionPool()
    {
        championPool = new Dictionary<Champion, int>();
        foreach(Champion champion in gameData.championArray)
        {
            int tierIndex = champion.cost - 1;
            if(tierIndex >= 0 && tierIndex < maxCopiesPerTier.Length)
            {
                championPool[champion] = maxCopiesPerTier[tierIndex];
            }
            else
            {
                championPool[champion] = 0; // Default to 0 copies if tier index is out of range
            }
        }
    }
    public void BuyLvl()
    {
        gamePlayController.BuyLvl();
    }

    // Use when sell champion
    public void ReturnChampionToPool(Champion champion)
    {
        if (championPool.ContainsKey(champion))
        {
            championPool[champion]++;
        }
    }
}

