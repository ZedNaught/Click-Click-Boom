//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
    public static Timer Instance;

    public float gameStartTime;
    public float GameTime {
        get {
            return Time.time - gameStartTime;
        }
    }

    [SerializeField]
    RectTransform[] digits = new RectTransform[3];
    int hundreds, tens, ones;
    bool started = false;

    void Start() {
        Instance = this;
    }

    public void StartTimer() {
        started = true;
        gameStartTime = Time.time;
    }

    void Update() {
        if (!started) {
            return;
        }
        int time = (int)GameTime;
        if (time == ones % 10) {
            return;
        }
        hundreds = (time / 100) % 10;
        tens = (time / 10) % 10;
        ones = time % 10;
        digits[0].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + hundreds];
        digits[1].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + tens];
        digits[2].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + ones];
    }
}