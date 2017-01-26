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

    bool firstClick;


    void Start() {
        Instance = this;
        firstClick = true;
        InitializeSprites();
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

    void InitializeBoard() {
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
        PlaceMines();
    }

    void Update() {
        SetCellUnderMouse();
        HandleInput();
    }

    void HandleInput() {
        if (Input.GetMouseButtonUp(0) && CellUnderMouse != null) {
            while(firstClick && CellUnderMouse.ContainsMine) {
                PlaceMines();
            }
            firstClick = false;
            RevealCell(CellUnderMouse);
        }

        // temporary // re-place mines on right-click
        if (Input.GetKeyUp(KeyCode.R)) {
            Debug.Log("re-placing mines due to \"R\" press");
            PlaceMines();
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
        if (cell.ContainsMine) {
            // TODO // handle game over
            Debug.Log("game over");
        }
        else if (GetAdjacentMineCount(cell) == 0) {
            foreach (Cell adjacentCell in GetAdjacentCells(cell)) {
                if (!adjacentCell.Revealed) {
                    RevealCell(adjacentCell);
                }
            }
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
}
