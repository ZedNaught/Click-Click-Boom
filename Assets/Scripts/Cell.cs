using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {
    enum CellState { Default, Revealed, Flagged, Suspect, Detonated }
    CellState cellState;
    bool _containsMine;
    public bool ContainsMine {
        get {
            return _containsMine;
        }
        set {
            _containsMine = value;

            // use flag sprite if cell contains mine to debug placement
            if (_containsMine) {
                spriteRenderer.sprite = Board.Instance.spritesDict["block_flag"];
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
    public bool Clickable {
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

    public int xPosition;
    public int yPosition;
    public bool Revealed {
        get {
            return cellState == CellState.Revealed;
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

    public void Highlight() {
        spriteRenderer.color = Color.yellow;
    }

    public void Reveal(int adjacentMineCount) {
        if (!Clickable) {
            return;
        }
        if (ContainsMine) {
            cellState = CellState.Detonated;
            spriteRenderer.sprite = Board.Instance.spritesDict["cell_bomb_detonated"];
        }
        else {
            cellState = CellState.Revealed;
            spriteRenderer.sprite = Board.Instance.spritesDict["cell_" + adjacentMineCount];
        }
    }
}
