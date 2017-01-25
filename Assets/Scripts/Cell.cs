using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {
    enum CellState { Default, Revealed, Flagged, Suspect }
    CellState cellState;
    bool _containsMine;
    public bool ContainsMine {
        get {
            return _containsMine;
        }
        set {
            _containsMine = value;
            if (_containsMine) {
                spriteRenderer.sprite = Board.Instance.spritesDict["cell_bomb_undetonated"];
            }
            else {
                spriteRenderer.sprite = Board.Instance.spritesDict["block"];
            }
        }
    }

    // interactability
    bool _underMouse;
    public bool UnderMouse {
        get {
            return _underMouse;
        }
        set {
            if (_underMouse && !value) {
                OffUnderMouse();
            }
            if (value) {
                OnUnderMouse();
            }
            _underMouse = value;
        }
    }
    static CellState[] clickableCellStates = { CellState.Default, CellState.Suspect };
    bool Clickable {
        get {
            bool clickable = false;
            foreach (CellState clickableState in clickableCellStates) {
                if (cellState == clickableState) {
                    clickable = true;
                    break;
                }
            }
            return clickable;
        }    
    }

    SpriteRenderer spriteRenderer;
    Color cellUnderMouseColor = new Color(0.93f, 0.93f, 0.93f);


    void OnEnable() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cellState = CellState.Default;
    }

    void OnUnderMouse() {
        spriteRenderer.color = cellUnderMouseColor;
    }

    void OffUnderMouse() {
        spriteRenderer.color = Color.white;
    }

    public void Click() {
        if (Clickable) {
            if (ContainsMine) {
                Debug.Log("MINE!");
            }
            spriteRenderer.sprite = Board.Instance.spritesDict["cell_0"];
            cellState = CellState.Revealed;
        }
    }
}
