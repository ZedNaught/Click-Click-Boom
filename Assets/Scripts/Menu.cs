using UnityEngine;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour {
    public void StartGame(string difficulty) {
        Board.Difficulty difficultyEnum;
        switch (difficulty) {
            case "beginner":
                difficultyEnum = Board.Difficulty.Beginner;
                break;
            case "intermediate":
                difficultyEnum = Board.Difficulty.Intermediate;
                break;
            case "expert":
                difficultyEnum = Board.Difficulty.Expert;
                break;
            default:
                Debug.Log("No valid difficulty specified.");
                return;
        }
        PlayerPrefs.SetInt("DifficultyEnum", (int)difficultyEnum);
        SceneManager.LoadSceneAsync("Game");
    }
}
