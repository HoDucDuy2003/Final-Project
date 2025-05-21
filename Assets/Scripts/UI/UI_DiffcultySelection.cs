using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static DifficultySettings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_DiffcultySelection : MonoBehaviour
{
    [SerializeField] private string mainGameScene = "MainGame";
    [SerializeField] private GameObject BG;
    public GameObject loadingScreen;
    public Slider loadingBarFill;
    [SerializeField] private float fillSpeed = 2f; // Speed of the fill animation
    public void SelectDifficulty(DifficultyLevel difficulty)
    {
        PlayerPrefs.SetString("Difficulty", difficulty.ToString());

        if (SceneUtility.GetBuildIndexByScenePath(mainGameScene) == -1)
        {
            Debug.LogError($"Scene '{mainGameScene}' not found in Build Settings!");
            return;
        }
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            BG.SetActive(false);
        }

        // Start loading the scene asynchronously
        StartCoroutine(LoadSceneAsync());
    }
    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(mainGameScene);
        float currentValue = 0f;
        // Optional: Update progress bar
        if (loadingBarFill != null)
        {
            while (!operation.isDone)
            {
                float progressValue = Mathf.Clamp01(operation.progress / 0.9f); // Target progress
                // Smoothly interpolate from current value to target progress
                currentValue = Mathf.Lerp(currentValue, progressValue, Time.deltaTime * fillSpeed);
                loadingBarFill.value = currentValue;
                yield return null;
            }

            // Ensure the slider reaches 100% smoothly at the end
            while (currentValue < 1f)
            {
                currentValue = Mathf.Lerp(currentValue, 1f, Time.deltaTime * fillSpeed);
                loadingBarFill.value = currentValue;
                yield return null;
            }
        }
        else
        {
            // Wait until the scene is fully loaded
            while (!operation.isDone)
            {
                yield return null;
            }
        }

        // Deactivate loading screen when done (optional)
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }
    public void SelectEasy()
    {
        SelectDifficulty(DifficultyLevel.Easy);
    }

    public void SelectNormal()
    {
        SelectDifficulty(DifficultyLevel.Normal);
    }

    public void SelectHard()
    {
        SelectDifficulty(DifficultyLevel.Hard);
    }
}
