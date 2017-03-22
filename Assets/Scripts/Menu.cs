using UnityEngine;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour {
    public void StartGame(string difficulty) {
        BoardUI.Difficulty difficultyEnum;
        switch (difficulty) {
            case "beginner":
                difficultyEnum = BoardUI.Difficulty.Beginner;
                break;
            case "intermediate":
                difficultyEnum = BoardUI.Difficulty.Intermediate;
                break;
            case "expert":
                difficultyEnum = BoardUI.Difficulty.Expert;
                break;
            default:
                Debug.Log("No valid difficulty specified.");
                return;
        }
        PlayerPrefs.SetInt("DifficultyEnum", (int)difficultyEnum);
        SceneManager.LoadSceneAsync("Game");
    }
}
