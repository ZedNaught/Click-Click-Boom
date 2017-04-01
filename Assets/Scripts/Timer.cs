using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
    public float gameStartTime;
    public float GameTime {
        get {
            return Time.time - gameStartTime;
        }
    }

    [SerializeField]
    Image[] timerDigits = new Image[3];
    bool started = false;
    int prevTimeElapsed = -1;

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
        for (int i = 0; i < timerDigits.Length; i++) {
            int digitValue = (timeElapsed / (int)Mathf.Pow(10, i)) % 10;
            int digitIndex = timerDigits.Length - (i + 1);
            timerDigits[digitIndex].sprite = Sprites.spritesDict["clock_" + digitValue];
        }
    }

    void Update() {
        if (!started) {
            return;
        }
        int timeElapsed = (int)GameTime;
        if (timeElapsed != prevTimeElapsed) {
            SetTimer(timeElapsed);
        }
        prevTimeElapsed = timeElapsed;
    }
}