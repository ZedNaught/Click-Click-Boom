using UnityEngine;
using System.Collections.Generic;

public class Board : MonoBehaviour {
    public static Board Instance;

    // sprites
    [SerializeField]
    Texture2D spritesheet;
    public Dictionary<string, Sprite> spritesDict;
    Sprite[] cellSprites = new Sprite[9];
    Sprite[] clockSprites = new Sprite[10];
    float cellSizeInUnits;
    Vector3 boardTopLeft;

    // difficulty
    struct DifficultySpec {
        public int width;
        public int height;
        public int mineCount;

        public DifficultySpec(int width, int height, int mineCount) {
            this.width = width;
            this.height = height;
            this.mineCount = mineCount;
        }
    }
//    DifficultySpec DIFFICULTY_BEGINNER = new DifficultySpec(8, 8, 10);
//    DifficultySpec DIFFICULTY_INTERMEDIATE = new DifficultySpec(16, 16, 40);
    DifficultySpec DIFFICULTY_EXPERT = new DifficultySpec(30, 16, 99);
    DifficultySpec currentDifficulty;

    // game objects
    [SerializeField]
    GameObject cellPrefab;
    Cell[,] cells;
    Cell _cellUnderMouse;
    Cell CellUnderMouse {
        get {
           return _cellUnderMouse;
        }
        set {
            if (_cellUnderMouse != null) {
                _cellUnderMouse.UnderMouse = false;
            }
            if (value != null) {
                value.UnderMouse = true;
            }
            _cellUnderMouse = value;
        }
    }

    bool freshBoard;


    void Start() {
        Instance = this;
        freshBoard = true;
        InitializeSprites();
        CreateBoard();
        InitializeBoard();
    }

    void PlaceMines() {
        foreach (Cell cell in cells) {
            cell.ContainsMine = false;
        }

        int numCells = currentDifficulty.width * currentDifficulty.height;
        List<int> selectedCells = Utilities.GetNRandomIndices(numCells, currentDifficulty.mineCount);
        foreach (int flatBoardIndex in selectedCells) {
            int xIndex = flatBoardIndex % currentDifficulty.width;
            int yIndex = flatBoardIndex / currentDifficulty.width;
            cells[yIndex, xIndex].ContainsMine = true;
        }
    }

    void InitializeSprites() {
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritesheet.name);
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

    void CreateBoard() {
        currentDifficulty = DIFFICULTY_EXPERT;
        cells = new Cell[currentDifficulty.height, currentDifficulty.width];
        cellSizeInUnits = cellSprites[0].rect.width / cellSprites[0].pixelsPerUnit;
        boardTopLeft = new Vector3(
            -(cellSizeInUnits * (currentDifficulty.width)) / 2f,
            (cellSizeInUnits * (currentDifficulty.height)) / 2f,
            0f
        );
        for (int dy = 0; dy < currentDifficulty.height; dy++) {
            for (int dx = 0; dx < currentDifficulty.width; dx++) {
                Vector3 cellPosition = boardTopLeft + (dx + 0.5f) * cellSizeInUnits * Vector3.right + (dy + 0.5f) * cellSizeInUnits * Vector3.down;
                Cell cell = ((GameObject) Instantiate(cellPrefab, cellPosition, Quaternion.identity, transform)).GetComponent<Cell>();
                cell.gameObject.name = "cell_" + dx + "_" + dy;
                cell.xPosition = dx;
                cell.yPosition = dy;
                cells[dy, dx] = cell;
            }
        }
    }

    void InitializeBoard() {
        foreach (Cell cell in cells) {
            cell.Reset();
        }
        PlaceMines();
        freshBoard = true;
    }

    void Update() {
        SetCellUnderMouse();
        HandleInput();
    }

    void HandleInput() {
        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space)) && CellUnderMouse != null) {
            while(freshBoard && CellUnderMouse.ContainsMine) {
                // while first click in game is on mine, re-place mines
                PlaceMines();
            }
            freshBoard = false;

            if (!CellUnderMouse.Revealed) {
                RevealCell(CellUnderMouse);
            }
            else {
                RevealAdjacentUnflaggedCells(CellUnderMouse);
            }
        }

        if ((Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.F)) && CellUnderMouse != null) {
            if (!CellUnderMouse.Revealed) {
                CellUnderMouse.ToggleFlag();
            }
            else {
                RevealAdjacentUnflaggedCells(CellUnderMouse);
            }
        }

        // temporary // re-place mines on right-click
        if (Input.GetKeyUp(KeyCode.P)) {
            if (freshBoard) {
                Debug.Log("re-placing mines due to \"P\" press");
                PlaceMines();
            }
            else {
                Debug.Log("cannot re-place mine after game has started");
            }
        }

        // temporary // re-place mines on right-click
        if (Input.GetKeyUp(KeyCode.R)) {
            Debug.Log("restarting game due to \"R\" press");
            InitializeBoard();
        }

        // temporary // debug adjacent cells code
        if (Input.GetKeyUp(KeyCode.A) && CellUnderMouse != null) {
            List<Cell> adjacentCells = GetAdjacentCells(CellUnderMouse);
            foreach (Cell cell in adjacentCells) {
                cell.Highlight();
            }
            Debug.Log("highlighted " + adjacentCells.Count + " adjacent mines due to \"A\" press");
        }

    }

    void RevealCell(Cell cell) {
        cell.Reveal(GetAdjacentMineCount(cell));
        if (cell.Detonated) {
            GameOver();
        }
        else if (GetAdjacentMineCount(cell) == 0) {
            foreach (Cell adjacentCell in GetAdjacentCells(cell)) {
                if (!adjacentCell.Revealed) {
                    RevealCell(adjacentCell);
                }
            }
        }
    }

    void GameOver() {
        foreach (Cell cell in cells) {
            cell.DoGameOverReveal();
        }
    }

    void SetCellUnderMouse() {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseDeltaScaled = (mousePosition - boardTopLeft) / cellSizeInUnits;
        int cellX = (int) Mathf.Floor(mouseDeltaScaled.x);
        int cellY = (int) Mathf.Floor(-mouseDeltaScaled.y);
        if (cellX >= 0 && cellX < currentDifficulty.width && cellY >= 0 && cellY < currentDifficulty.height) {
            CellUnderMouse = cells[cellY, cellX];
        }
        else {
            CellUnderMouse = null;
        }
    }

    List<Cell> GetAdjacentCells(Cell cell) {
        // TODO // could probably be optimized
        List<Cell> adjacentCells = new List<Cell>();
        for (int y = cell.yPosition - 1; y <= cell.yPosition + 1; y++) {
            for (int x = cell.xPosition - 1; x <= cell.xPosition + 1; x++) {
                // make sure cell at coordinates exists and isn't the input cell
                if ((x != cell.xPosition || y != cell.yPosition) &&
                        x >= 0 && x < currentDifficulty.width &&
                        y >= 0 && y < currentDifficulty.height) {
                    adjacentCells.Add(cells[y, x]);
                }
            }
        }

        return adjacentCells;
    }

    int GetAdjacentMineCount(Cell cell) {
        int numAdjacentMines = 0;
        List<Cell> adjacentCells = GetAdjacentCells(cell);
        foreach (Cell adjacentCell in adjacentCells) {
            if (adjacentCell.ContainsMine) {
                numAdjacentMines++;
            }
        }
        return numAdjacentMines;
    }

    int GetAdjacentFlagCount(Cell cell) {
        int numAdjacentFlags = 0;
        List<Cell> adjacentCells = GetAdjacentCells(cell);
        foreach (Cell adjacentCell in adjacentCells) {
            if (adjacentCell.Flagged) {
                numAdjacentFlags++;
            }
        }
        return numAdjacentFlags;
    }

    void RevealAdjacentUnflaggedCells(Cell cell) {
        if (GetAdjacentMineCount(cell) == GetAdjacentFlagCount(cell)) {
            foreach (Cell adjacentCell in GetAdjacentCells(cell)) {
                if (!adjacentCell.Flagged && !adjacentCell.Revealed && !adjacentCell.Detonated) {
                    RevealCell(adjacentCell);
                }
            }
        }
    }
}
