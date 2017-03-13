//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour {
    public static Timer Instance;
    [SerializeField]
    GameObject digitPrefab;

    public float gameStartTime;
    public float GameTime {
        get {
            if (!Board.Instance.GameStarted) {
                return 0f;
            }
            return Time.time - gameStartTime;
        }
    }

    Sprite[] sprites = new Sprite[3];

    void Start() {
        Instance = this;
        InitializeDigits();
    }

    void InitializeDigits() {
        Sprite zeroSprite = Sprites.clockSprites[0];
        float digitWidthInUnits = zeroSprite.rect.width / zeroSprite.pixelsPerUnit;
        for (int i = 0; i < sprites.Length; i++) {
            sprites[i] = ((GameObject) Instantiate(digitPrefab, Vector3.zero, Quaternion.identity, transform)).GetComponent<Sprite>();
        }
    }
}