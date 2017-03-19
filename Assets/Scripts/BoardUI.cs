using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour {
    public static BoardUI Instance;
    public int revealedCells;
    [SerializeField]
    RectTransform[] mineCountDigits = new RectTransform[3];

    // difficulty
    struct DifficultySpec {
        public int width;
        public int height;
        public int mineCount;
        public int totalCells;
        public int emptyCells;

        public DifficultySpec(int width, int height, int mineCount) {
            this.width = width;
            this.height = height;
            this.mineCount = mineCount;
            this.totalCells = width * height;
            this.emptyCells = this.totalCells - mineCount;
        }
    }
    static DifficultySpec DIFFICULTY_BEGINNER = new DifficultySpec(8, 8, 10);
    static DifficultySpec DIFFICULTY_INTERMEDIATE = new DifficultySpec(16, 16, 40);
    static DifficultySpec DIFFICULTY_EXPERT = new DifficultySpec(30, 16, 99);
    DifficultySpec currentDifficulty;
    public enum Difficulty { Beginner, Intermediate, Expert };
    Dictionary<Difficulty, DifficultySpec> difficultyMap = new Dictionary<Difficulty, DifficultySpec>() {
        {Difficulty.Beginner, DIFFICULTY_BEGINNER},
        {Difficulty.Intermediate, DIFFICULTY_INTERMEDIATE},
        {Difficulty.Expert, DIFFICULTY_EXPERT},
    };

    // game objects
    [SerializeField]
    GameObject cellPrefab;
    [SerializeField]
    RectTransform cellContainer;
    CellUI[,] cells;
    [SerializeField]
    public Image faceButtonImage;

    bool _freshBoard;
    public bool FreshBoard {
        get { return _freshBoard; }
        set {
            _freshBoard = value;
            if (!_freshBoard) {
                Timer.Instance.StartTimer();
            }
            else {
                Timer.Instance.ResetTimer();
                faceButtonImage.sprite = Sprites.spritesDict["face_smile"];
            }
        }
    }
    public bool GameStarted {
        get { return !FreshBoard; }
    }

    void Start() {
        Instance = this;
        CreateBoard();
        InitializeBoard();
        UpdateMineCount();
    }

    public void PlaceMines() {
        foreach (CellUI cell in cells) {
            cell.ContainsMine = false;
        }

        int numCells = currentDifficulty.width * currentDifficulty.height;
        List<int> selectedCells = Utilities.GetNRandomIndices(numCells, currentDifficulty.mineCount);
        foreach (int flatBoardIndex in selectedCells) {
            int xIndex = flatBoardIndex % currentDifficulty.width;
            int yIndex = flatBoardIndex / currentDifficulty.width;
            cells[yIndex, xIndex].ContainsMine = true;
        }

        foreach (CellUI cell in cells) {
            cell.adjacentMineCount = GetAdjacentMineCount(cell);
        }
    }

    void CreateBoard() {
        Difficulty difficultyEnum = (Difficulty)PlayerPrefs.GetInt("DifficultyEnum", (int)Difficulty.Expert);
        currentDifficulty = difficultyMap[difficultyEnum];
        cells = new CellUI[currentDifficulty.height, currentDifficulty.width];

        Vector2 cellContainerSize = new Vector2(currentDifficulty.width * 16, currentDifficulty.height * 16);
        cellContainer.sizeDelta = cellContainerSize;
        Vector2 boardSize = new Vector2(32, 64) + cellContainerSize;
        ((RectTransform)transform).sizeDelta = boardSize;
        GridLayoutGroup layoutGroup = cellContainer.GetComponent<GridLayoutGroup>();
        layoutGroup.constraintCount = currentDifficulty.width;

        foreach (Transform child in cellContainer.transform) {
            Destroy(child.gameObject);
        }

        for (int dy = 0; dy < currentDifficulty.height; dy++) {
            for (int dx = 0; dx < currentDifficulty.width; dx++) {
                CellUI cell = ((GameObject) Instantiate(cellPrefab, cellContainer)).GetComponent<CellUI>();
                cell.board = this;
                cell.transform.localScale = Vector3.one;
                cell.gameObject.name = "cell_" + dx + "_" + dy;
                cell.xPosition = dx;
                cell.yPosition = dy;
                cells[dy, dx] = cell;
            }
        }
    }

    void InitializeBoard() {
        foreach (CellUI cell in cells) {
            cell.Reset();
        }
        PlaceMines();
        FreshBoard = true;
        GameManager.Instance.gameOver = false;
        revealedCells = 0;
        UpdateMineCount();
    }

    public void RestartGame() {
        Timer.Instance.StopTimer();
        InitializeBoard();
    }

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        if (!GameManager.Instance.gameOver && Input.GetMouseButtonUp(0)) {
            faceButtonImage.sprite = Sprites.spritesDict["face_smile"];
        }

        // restart with R key
        if (Input.GetKeyUp(KeyCode.R)) {
            Debug.Log("restarting game due to \"R\" press");
            RestartGame();
        }

        // exit to main menu with M key
        if (Input.GetKeyUp(KeyCode.M)) {
            SceneManager.LoadSceneAsync("Menu");
        }

        if (Config.ENABLE_DEBUG_COMMANDS) {
            if (Input.GetKeyUp(KeyCode.P)) {
                if (FreshBoard) {
                    Debug.Log("re-placing mines due to \"P\" press");
                    PlaceMines();
                }
                else {
                    Debug.Log("cannot re-place mines after game has started");
                }
            }
        }
    }

    public void CheckIfGameWon() {
        Debug.Log("checking if game won");
        if (revealedCells == currentDifficulty.emptyCells) {
            DoGameVictory();
        }
    }

    public void DoGameOver() {
        GameManager.Instance.gameOver = true;
        Timer.Instance.StopTimer();
        foreach (CellUI cell in cells) {
            cell.DoGameOverReveal();
        }
        faceButtonImage.sprite = Sprites.spritesDict["face_dead"];
    }

    void DoGameVictory() {
        Debug.Log(string.Format("You win! Time: {0:0.00} seconds.", Timer.Instance.GameTime));
        GameManager.Instance.gameOver = true;
        Timer.Instance.StopTimer();
        foreach (CellUI cell in cells) {
            cell.DoGameOverReveal();
        }
        faceButtonImage.sprite = Sprites.spritesDict["face_cool"];
    }

    public void UpdateMineCount() {
        int mineCount = currentDifficulty.mineCount - GetFlagCount();
        mineCount = Mathf.Max(0, mineCount);
        int hundreds = (mineCount / 100) % 10;
        int tens = (mineCount / 10) % 10;
        int ones = mineCount % 10;
        mineCountDigits[0].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + hundreds];
        mineCountDigits[1].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + tens];
        mineCountDigits[2].GetComponent<Image>().sprite = Sprites.spritesDict["clock_" + ones];
    }

    List<CellUI> GetAdjacentCells(CellUI centerCell, bool skipFlagged = false) {
        // TODO // could probably be optimized
        List<CellUI> adjacentCells = new List<CellUI>();
        for (int y = centerCell.yPosition - 1; y <= centerCell.yPosition + 1; y++) {
            for (int x = centerCell.xPosition - 1; x <= centerCell.xPosition + 1; x++) {
                // make sure cell at coordinates exists and isn't the input cell
                if ((x != centerCell.xPosition || y != centerCell.yPosition) &&
                        x >= 0 && x < currentDifficulty.width &&
                        y >= 0 && y < currentDifficulty.height) {
                    CellUI adjacentCell = cells[y, x];
                    if (skipFlagged && adjacentCell.Flagged) {
                        continue;
                    }
                    adjacentCells.Add(adjacentCell);
                }
            }
        }

        return adjacentCells;
    }

    int GetAdjacentMineCount(CellUI cell) {
        int numAdjacentMines = 0;
        List<CellUI> adjacentCells = GetAdjacentCells(cell);
        foreach (CellUI adjacentCell in adjacentCells) {
            if (adjacentCell.ContainsMine) {
                numAdjacentMines++;
            }
        }
        return numAdjacentMines;
    }

    int GetAdjacentFlagCount(CellUI cell) {
        int numAdjacentFlags = 0;
        List<CellUI> adjacentCells = GetAdjacentCells(cell);
        foreach (CellUI adjacentCell in adjacentCells) {
            if (adjacentCell.Flagged) {
                numAdjacentFlags++;
            }
        }
        return numAdjacentFlags;
    }

    public void RevealAdjacentUnflaggedCells(CellUI cell) {
        if (cell.adjacentMineCount == GetAdjacentFlagCount(cell)) {
            List<CellUI> adjacentUnflaggedCells = GetAdjacentCells(cell, skipFlagged: true);
            // special pass that will only detonate mines
            // prevents revealing safe cells before detonation
            foreach (CellUI adjacentCell in adjacentUnflaggedCells) {
                if (adjacentCell.ContainsMine) {
                    adjacentCell.Reveal();
                    return;
                }
            }
            foreach (CellUI adjacentCell in adjacentUnflaggedCells) {
                if (adjacentCell.Clickable) {
                    adjacentCell.Reveal();
                }
            }
        }
    }

    int GetFlagCount() {
        int count = 0;
        foreach (CellUI cell in cells) {
            if (cell.Flagged) {
                count += 1;
            }
        }
        return count;
    }

    void UpdateTimer() {
        
    }
}
