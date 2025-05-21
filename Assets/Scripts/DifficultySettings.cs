using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultySettings : Singleton<DifficultySettings>
{
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }

    [System.Serializable]
    public struct DifficultyConfig
    {
        public int damageWhenPlayerLose; // adjust for each difficult level in AIEnmey
        public int startingGold; // adjsut for player to start in each difficult level
    }

    [SerializeField] private DifficultyConfig[] difficultyConfigs;

    
    public DifficultyConfig GetConfig(DifficultyLevel level)
    {
        int index = (int)level;
        if (index >= 0 && index < difficultyConfigs.Length)
        {
            return difficultyConfigs[index];
        }
        Debug.LogWarning($"Difficulty level {level} not found, returning default config.");
        return new DifficultyConfig(); // Trả về cấu hình mặc định nếu lỗi
    }
}