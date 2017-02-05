using UnityEngine;
//using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    void Start() {
        Instance = this;
    }
}
