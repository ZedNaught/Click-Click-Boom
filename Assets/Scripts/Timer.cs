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

    public void StopTimer() {
        started = false;
    }

    public void ResetTimer() {
        SetTimer(0);
    }

    void SetTimer(int timeElapsed) {
        // TODO // this could be done in a simple loop i think
        hundreds = (timeElapsed / 100) % 10;
        tens = (timeElapsed / 10) % 10;
        ones = timeElapsed % 10;
        digits[0].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + hundreds];
        digits[1].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + tens];
        digits[2].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + ones];
    }

    void Update() {
        if (!started) {
            return;
        }
        int timeElapsed = (int)GameTime;
        if (timeElapsed == ones % 10) {
            return;
        }
        SetTimer(timeElapsed);
    }
}