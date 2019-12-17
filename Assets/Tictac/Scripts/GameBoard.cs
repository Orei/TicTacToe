using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Used to calculate the best move using the minimax algorithm.
/// </summary>
public struct ScoredMove
{
    public int Move;
    public int Score;
}

[RequireComponent(typeof(ObjectPool))]
public class GameBoard : MonoBehaviour
{
    /// <summary>
    /// Set of all winning steps, a step is described by increase in cells from start cell (i.e {0, 1} being vertical).
    /// </summary>
    public static int[][] WINNING_STEPS { get; } = new int[][]
    {
        new [] { 1,  0 }, // Horizontal step
        new [] { 0,  1 }, // Vertical step
        new [] { 1,  1 }, // Diagonal left-up step
        new [] { 1, -1 }, // Diagonal left-down step
    };

    /// <summary>
    /// Set of all winning lines.
    /// </summary>
    public static int[][] WINNING_LINES { get; private set; } = null;
    
    /// <summary>
    /// Value describing an unoccupied cell.
    /// </summary>
    public static int EMPTY { get; }= -1;

    /// <summary>
    /// Array of integers representing the board cells.
    /// </summary>
    public int[] Cells { get; private set; } = null;

    [Tooltip("Prefab representing a cell object.")]
    [SerializeField] private GameObject cellPrefab = null;
    [Tooltip("Adds spacing between each cell.")]
    [SerializeField] private float spacing = 0.1f;
    [Tooltip("Size of the cell grid.")]
    [SerializeField] private int size = 4;
    [Tooltip("Number of cells required in a row to win.")]
    [SerializeField] private int numWinningCells = 4;
    private ObjectPool pool = null; 

    private void Awake()
    {
        pool = GetComponent<ObjectPool>();

        Debug.Assert(pool != null, "Unable to get/create ObjectPool component.");
    }

    /// <summary>
    /// Creates a new empty board.
    /// </summary>
    /// <param name="players">Array of players.</param>
    public void Create(Player[] players)
    {
        Cells = new int[size * size];
        pool.Create(Cells.Length, cellPrefab, Mathf.CeilToInt(Cells.Length / 2f), players);
        Reset();

        // Create the winning lines array
        List<int[]> lines = new List<int[]>();
        for (int i = 0; i < Cells.Length; i++)
        {
            // Go through all steps, which are basically directions
            for (int j = 0; j < WINNING_STEPS.Length; j++)
            {
                // We're stepping in this direction
                int stepX = WINNING_STEPS[j][0];
                int stepY = WINNING_STEPS[j][1];
                int[] line = GetStepIndices(i, stepX, stepY);

                if (line != null)
                    lines.Add(line);
            }
        }

        WINNING_LINES = lines.ToArray();
    }

    /// <summary>
    /// Resets the game board.
    /// </summary>
    public void Reset()
    {
        // If the board hasn't been created yet
        if (Cells == null || pool == null)
            return;

        // Reset cells
        for (int i = 0; i < Cells.Length; i++)
            Cells[i] = EMPTY;

        // Reset the object pool as well
        pool.Reset();

        // Place tiles
        for (int i = 0; i < Cells.Length; i++)
        {
            GameObject tile = pool.GetCell();
            tile.transform.position = GetCellPosition(i);
            
            // Update tile index, used to identify which cell we want to place
            CellData data = tile.GetComponent<CellData>();
            data?.SetIndex(i);
        }
    }

    /// <summary>
    /// Tries to set the value of a cell, will not override an occupied cell.
    /// </summary>
    /// <param name="player">Index of the player within the player array.</param>
    /// <returns>True if the cell was available, false otherwise.</returns>
    public bool Set(int cell, int player)
    {
        if (!IsEmpty(cell))
            return false;
        
        // Set cell value
        Cells[cell] = player;

        // Enable and move piece
        GameObject piece = pool.GetPiece(player);
        piece.transform.position = GetCellPosition(cell) + Vector3.up * 0.1f;

        return true;
    }

    /// <summary>
    /// Gets the value of a cell, -1 if invalid or empty.
    /// </summary>
    public int Get(int cell)
    {
        if (!Contains(cell))
            return -1;

        return Cells[cell];
    }

    /// <summary>
    /// Whether the game board is currently in a terminal state; the game should end.
    /// </summary>
    /// <param name="winner">Index of the winning player, -1 if none.</returns>
    /// <returns>True if the game should terminate, false otherwise.</returns>
    public bool IsTerminalState(out int winner)
    {
        // If the board contains a winning line, the game has ended
        // NOTE: Winner is set by outing the value of the line terminal check
        for (int i = 0; i < WINNING_LINES.Length; i++)
            if (IsLineTerminal(out winner, WINNING_LINES[i]))
                return true;

        // Invalid winner, since nobody has terminal lines
        winner = -1;

        // If all cells are occupied, tie
        return IsAllOccupied();
    }

    /// <summary>
    /// Returns all parts of a step from cell, described as an array of indices.
    /// </summary>
    public int[] GetStepIndices(int cell, int stepX, int stepY)
    {
        int[] indices = new int[numWinningCells];

        // Travelling across y by stepping x
        if (cell % size + stepX * numWinningCells > size)
            return null;

        for (int i = 0; i < indices.Length; i++)
        {
            int x = stepX * i;
            int y = stepY * i * size;
            int index = cell + x + y;

            // Invalid index
            if (!Contains(index))
                return null;

            indices[i] = index;
        }

        return indices;
    }

    /// <summary>
    /// Whether a full line within the game board has been occupied, resulting in a winning line.
    /// </summary>
    /// <param name="winner">Index of the winning player, -1 if none.</returns>
    /// <param name="indices">Indices within game board to check as a line.</returns>
    /// <returns>True if the line is terminal, false otherwise.</returns>
    public bool IsLineTerminal(out int winner, params int[] indices)
    {
        // Invalid index to begin with
        winner = -1;

        // Checking a single value or no value as a line, not on my watch
        if (indices.Length <= 1)
            return false;

        // Check each index against the next
        for (int i = 0; i < indices.Length - 1; i++)
        {
            // Get the current and next index from indices
            int index = indices[i];
            int next = indices[i + 1];

            // Make sure the cells actually exists
            if (IsEmpty(index) || IsEmpty(next))
                return false;

            // Cells don't match
            if (Cells[index] != Cells[next])
                return false;
        }

        // Get the owning index of the first cell, since all cells must match to be terminal
        winner = Cells[indices[0]];
        return true;
    }

    /// <summary>
    /// Whether all cells have been occupied.
    /// </summary>
    public bool IsAllOccupied()
    {
        // Search for an empty cell
        for (int i = 0; i < Cells.Length; i++)
            if (Cells[i] == EMPTY)
                return false;

        return true;
    }

    /// <summary>
    /// Whether all cells are empty.
    /// </summary>
    public bool IsAllEmpty()
    {
        // Search for an occupied cell
        for (int i = 0; i < Cells.Length; i++)
            if (Cells[i] != EMPTY)
                return false;

        return true;
    }

    /// <summary>
    /// Whether the board contains the cell.
    /// </summary>
    public bool Contains(int cell)
    {
        return cell >= 0 && cell < Cells.Length;
    }

    /// <summary>
    /// Whether the cell is empty and valid.
    /// </summary>
    public bool IsEmpty(int cell)
    {
        return Contains(cell) && Cells[cell] == EMPTY;
    }

    /// <summary>
    /// Returns the world position of a cell.
    /// </summary>
    public Vector3 GetCellPosition(int cell)
    {
        // Position from index
        int x = cell % size;
        int y = cell / size;

        // Size of the prefab, or one unit by default if none, ignore y
        BoxCollider collider = cellPrefab.GetComponent<BoxCollider>();
        Vector3 prefabSize = collider != null ? collider.size : Vector3.one;

        // Offset the board, centering and adding spacing around the tiles
        float halfSize = (size - 1) / 2f;
        Vector3 position = new Vector3(x, 0f, y);
        Vector3 margin = new Vector3(x - halfSize, 0f, y - halfSize) * spacing;
        Vector3 offset = new Vector3(prefabSize.x, 0f, prefabSize.z) * halfSize;

        return position - offset + margin;
    }

    /// <summary>
    /// Finds the best move according to the Minimax algorithm.
    /// Use default values when calling manually.
    /// </summary>
    /// <param name="maximizer">Maximizing player, solving for.</returns>
    /// <param name="minimizer">Minimizing player, solving against.</returns>
    /// <param name="isMax">Whether this move is a maximizing move, i.e simulating maximizing player.</returns>
    /// <param name="depth">Depth of the current simulation.</returns>
    public ScoredMove Minimax(int maximizer, int minimizer, bool isMax = true, int depth = 0)
    {
        ScoredMove bestMove = new ScoredMove
        {
            Move = -1,
            Score = isMax ? -1000 : 1000
        };

        // If we're in a terminal state, return heuristic score
        if (IsTerminalState(out _))
        {
            bestMove.Score = Heuristic(maximizer, minimizer, depth);
            return bestMove;
        }

        for (int i = 0; i < Cells.Length; i++)
        {
            // Store the original value of the cell
            int cell = Cells[i];

            // Cell is already occupied
            if (!IsEmpty(i))
                continue;

            // When maximizing, we're simulating the players move
            // minimizing simulates the opponent, here we make that move on the board
            Cells[i] = isMax ? maximizer : minimizer;

            // With our modified board, recursively check every possibility
            // Flip isMax, since users take one turn each and increase depth
            ScoredMove move = Minimax(maximizer, minimizer, !isMax, depth + 1);

            // Take the best move, maximizing - high, minimizing - low
            if (isMax)
            {
                if (move.Score >= bestMove.Score)
                {
                    bestMove.Score = move.Score;
                    bestMove.Move = i;
                }
            }
            else
            {
                if (move.Score <= bestMove.Score)
                {
                    bestMove.Score = move.Score;
                    bestMove.Move = i;
                }
            }

            // Undo our change to the board
            Cells[i] = cell;
        }

        return bestMove;
    }

    /// <summary>
    /// Returns the score for maximizer by evaluating win/lose/tie conditions.
    /// </summary>
    private int Heuristic(int maximizer, int minimizer, int depth)
    {
        if (IsTerminalState(out int winner))
        {
            // Winning condition, increases score
            if (winner == maximizer)
                return 10 - depth;
            // Losing condition, decreases score
            else if (winner == minimizer)
                return depth - 10;
        }

        // No winner, no score
        return 0;
    }
}