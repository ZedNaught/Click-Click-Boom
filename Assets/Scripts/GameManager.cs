using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public bool gameOver = false;

    public void GoToMenu() {
        SceneManager.LoadSceneAsync("Menu");
    }
}
