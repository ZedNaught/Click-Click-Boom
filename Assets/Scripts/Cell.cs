﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

enum CellState { Default, Revealed, Flagged, Suspect, Detonated, RevealUndetonated,
                 FlaggedWrong }

public class Cell : MonoBehaviour,
                      IPointerEnterHandler,
                      IPointerExitHandler,
                      IPointerClickHandler,
                      IPointerDownHandler {
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
                image.sprite = sprite;
            }
        }
    }
    bool _containsMine;
    public bool ContainsMine {
        get { return _containsMine; }
        set {
            _containsMine = value;
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

    Image image;
    Color cellUnderMouseColor = new Color(0.93f, 0.93f, 0.93f);
    bool underMouse = false;
    public Board board;
    public int adjacentMineCount;


    void OnEnable() {
        image = GetComponent<Image>();
    }

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        if (!underMouse || GameManager.Instance.gameOver) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            ToggleFlag();
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            Click();
        }
    }

    public void Click() {
        if (Clickable) {
            Reveal();
        }
        else if (Revealed) {
            board.RevealAdjacentUnflaggedCells(this);
        }
        Board.Instance.CheckIfGameWon();
    }

    public void Reveal() {
        if (GameManager.Instance.gameOver) {
            return;
        }

        if (board.FreshBoard) {
            while(board.FreshBoard && ContainsMine) {
                // while first click in game is on mine, re-place mines
                board.PlaceMines();
            }
            board.FreshBoard = false;
        }

        if (ContainsMine) {
            CellState = CellState.Detonated;
            board.DoGameOver();
        }
        else {
            CellState = CellState.Revealed;
            Board.Instance.revealedCells++;
            image.sprite = Sprites.spritesDict["cell_" + adjacentMineCount];
            if (adjacentMineCount == 0) {
                board.RevealAdjacentUnflaggedCells(this);
            }
        }
    }

    public void ToggleFlag() {
        if (GameManager.Instance.gameOver || Revealed) {
            return;
        }

        if (CellState == CellState.Default) {
            CellState = CellState.Flagged;
        }
        else if (CellState == CellState.Flagged) {
            CellState = CellState.Default;
        }
        Board.Instance.UpdateMineCount();
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

    public void OnPointerEnter(PointerEventData eventData) {
        underMouse = true;
        image.color = cellUnderMouseColor;
    }

    public void OnPointerExit(PointerEventData eventData) {
        underMouse = false;
        image.color = Color.white;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (GameManager.Instance.gameOver) {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left && !Input.GetMouseButton(1)) {
            Click();
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (GameManager.Instance.gameOver) {
            return;
        }

        if (Clickable && eventData.button == PointerEventData.InputButton.Left) {
            Board.Instance.faceButtonImage.sprite = Sprites.spritesDict["face_scared"];
        }

        else if (eventData.button == PointerEventData.InputButton.Right) {
            ToggleFlag();
        }
    }
}
