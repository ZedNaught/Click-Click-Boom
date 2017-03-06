﻿using System.Collections.Generic;
using UnityEngine;

public class Sprites : MonoBehaviour {
    // sprites
//    [SerializeField]
//    static Texture2D spritesheet;
    public static Dictionary<string, Sprite> spritesDict;
    public static Sprite[] cellSprites = new Sprite[9];
    public static Sprite[] clockSprites = new Sprite[10];

    void Awake() {
        Debug.Log("Sprites.Awake()");
        InitializeSprites();
    }

    void InitializeSprites() {
        Debug.Log("Sprites.InitializeSprites()");
        Sprite[] sprites = Resources.LoadAll<Sprite>("minesweeper_spritesheet");
        spritesDict = new Dictionary<string, Sprite>();
        foreach (Sprite s in sprites) {
            spritesDict[s.name] = s;
        }
        for (int i = 0; i < cellSprites.Length; i++) {
            cellSprites[i] = spritesDict["cell_" + i];
        }
        for (int i = 0; i < clockSprites.Length; i++) {
            clockSprites[i] = spritesDict["clock_" + i];
        }
    }
}