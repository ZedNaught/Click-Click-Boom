using UnityEngine;
using System.Collections;

enum CellState { Default, Revealed, Flagged, Suspect, Detonated, RevealUndetonated,
                 FlaggedWrong }

public class Cell : MonoBehaviour {
    CellState _cellState;
    CellState CellState {
        get { return _cellState; }
        set {
            _cellState = value;
            Sprite sprite = null;
            switch (_cellState) {
                case CellState.Default:
                    sprite = Sprites.spritesDict["block"];
                    break;
                case CellState.Revealed:
                    // sprite change for this case is currently handled elsewhere
                    break;
                case CellState.Flagged:
                    sprite = Sprites.spritesDict["block_flag"];
                    break;
                case CellState.Suspect:
                    sprite = Sprites.spritesDict["block_question"];
                    break;
                case CellState.Detonated:
                    sprite = Sprites.spritesDict["cell_bomb_detonated"];
                    break;
                case CellState.RevealUndetonated:
                    sprite = Sprites.spritesDict["cell_bomb_undetonated"];
                    break;
                case CellState.FlaggedWrong:
                    sprite = Sprites.spritesDict["cell_bomb_wrong"];
                    break;
            }
            if (sprite != null) {
                spriteRenderer.sprite = sprite;
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
            foreach (CellState clickableState in clickableCellStates) {
                if (CellState == clickableState) {
                    return true;
                }
            }
            return false;
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
            spriteRenderer.sprite = Sprites.spritesDict["cell_" + adjacentMineCount];
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

    public void DoGameOverReveal() {
        if (ContainsMine && (CellState == CellState.Default || CellState == CellState.Suspect)) {
            CellState = CellState.RevealUndetonated;
        }
        else if (!ContainsMine && CellState == CellState.Flagged) {
            CellState = CellState.FlaggedWrong;
        }
    }

    public void Reset() {
        CellState = CellState.Default;
    }
}
