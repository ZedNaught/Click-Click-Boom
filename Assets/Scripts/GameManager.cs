using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public bool gameOver = false;

    void Start() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToMenu() {
        SceneManager.LoadSceneAsync("Menu");
    }
}
