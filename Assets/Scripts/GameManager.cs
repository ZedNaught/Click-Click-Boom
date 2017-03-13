using UnityEngine;
//using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public bool gameOver = false;

    void Start() {
        Instance = this;
    }
}
