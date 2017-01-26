using UnityEngine;
using System.Collections;

enum CellState { Default, Revealed, Flagged, Suspect, Detonated }

public class Cell : MonoBehaviour {
    CellState _cellState;
    CellState CellState {
        get { return _cellState; }
        set {
            _cellState = value;
            if (_cellState == CellState.Detonated) {
                spriteRenderer.sprite = Board.Instance.spritesDict["cell_bomb_detonated"];
            }
            else if (_cellState == CellState.Flagged) {
                spriteRenderer.sprite = Board.Instance.spritesDict["block_flag"];
            }
            else if (_cellState == CellState.Default) {
                spriteRenderer.sprite = Board.Instance.spritesDict["block"];
            }
        }
    }
    bool _containsMine;
    public bool ContainsMine {
        get { return _containsMine; }
        set {
            _containsMine = value;

            // use flag sprite if cell contains mine to debug placement
//            if (_containsMine && CellState == CellState.Default) {
//                CellState = CellState.Flagged;
//            }
//            else if (CellState == CellState.Flagged) {
//                CellState = CellState.Default;
//            }
        }
    }
    public bool Revealed {
        get {
            return CellState == CellState.Revealed;
        }
    }
    public bool Detonated {
        get {
            return CellState == CellState.Detonated;
        }
    }
    public bool Flagged {
        get {
            return CellState == CellState.Flagged;
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
    public bool Clickable {
        get {
            bool clickable = false;
            foreach (CellState clickableState in clickableCellStates) {
                if (CellState == clickableState) {
                    clickable = true;
                    break;
                }
            }
            return clickable;
        }
    }

    public int xPosition;
    public int yPosition;


    SpriteRenderer spriteRenderer;
    Color cellUnderMouseColor = new Color(0.93f, 0.93f, 0.93f);


    void OnEnable() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CellState = CellState.Default;
    }

    void OnUnderMouse() {
        spriteRenderer.color = cellUnderMouseColor;
    }

    void OffUnderMouse() {
        spriteRenderer.color = Color.white;
    }

    public void Highlight() {
        spriteRenderer.color = Color.yellow;
    }

    public void Reveal(int adjacentMineCount) {
        if (!Clickable) {
            return;
        }
        if (ContainsMine) {
            CellState = CellState.Detonated;
        }
        else {
            CellState = CellState.Revealed;
            spriteRenderer.sprite = Board.Instance.spritesDict["cell_" + adjacentMineCount];
        }
    }

    public void ToggleFlag() {
        if (CellState == CellState.Default) {
            CellState = CellState.Flagged;
        }
        else if (CellState == CellState.Flagged) {
            CellState = CellState.Default;
        }
    }
}
