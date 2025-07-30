using ConwayGameOfLife.Core.Models;

namespace ConwayGameOfLife.Core.Services;

/// <summary>
/// Service for Conway's Game of Life logic
/// </summary>
public class GameOfLifeService : IGameOfLifeService
{
    /// <summary>
    /// The maximum number of iterations to compute when looking for a stable state
    /// </summary>
    private const int MaxIterations = 1000;

    /// <summary>
    /// Computes the next generation of the board
    /// </summary>
    /// <param name="board">The current board state</param>
    /// <returns>The next generation of the board</returns>
    public Board ComputeNextGeneration(Board board)
    {
        var nextBoard = new Board(board.Width, board.Height);
        
        // Use parallel processing for large boards (more than 50x50)
        if (board.Width * board.Height > 2500)
        {
            Parallel.For(0, board.Width, x =>
            {
                for (int y = 0; y < board.Height; y++)
                {
                    ComputeNextCellState(board, nextBoard, x, y);
                }
            });
        }
        else
        {
            // Use sequential processing for smaller boards
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    ComputeNextCellState(board, nextBoard, x, y);
                }
            }
        }

        return nextBoard;
    }
    
    /// <summary>
    /// Computes the next state of a single cell
    /// </summary>
    /// <param name="currentBoard">The current board state</param>
    /// <param name="nextBoard">The next board state</param>
    /// <param name="x">The x-coordinate of the cell</param>
    /// <param name="y">The y-coordinate of the cell</param>
    private void ComputeNextCellState(Board currentBoard, Board nextBoard, int x, int y)
    {
        int liveNeighbors = CountLiveNeighbors(currentBoard, x, y);
        bool isCurrentlyAlive = currentBoard.Cells[x, y].IsAlive;

        // Apply Conway's Game of Life rules
        if (isCurrentlyAlive && (liveNeighbors < 2 || liveNeighbors > 3))
        {
            // Rule 1 & 3: Any live cell with fewer than two or more than three live neighbors dies
            nextBoard.Cells[x, y].IsAlive = false;
        }
        else if (isCurrentlyAlive && (liveNeighbors == 2 || liveNeighbors == 3))
        {
            // Rule 2: Any live cell with two or three live neighbors lives on
            nextBoard.Cells[x, y].IsAlive = true;
        }
        else if (!isCurrentlyAlive && liveNeighbors == 3)
        {
            // Rule 4: Any dead cell with exactly three live neighbors becomes a live cell
            nextBoard.Cells[x, y].IsAlive = true;
        }
        else
        {
            // Otherwise, the cell remains in its current state
            nextBoard.Cells[x, y].IsAlive = isCurrentlyAlive;
        }
    }

    /// <summary>
    /// Computes the board state after N generations
    /// </summary>
    /// <param name="board">The current board state</param>
    /// <param name="generations">The number of generations to compute</param>
    /// <returns>The board state after N generations</returns>
    public Board ComputeGenerations(Board board, int generations)
    {
        if (generations <= 0)
        {
            return board.Clone();
        }

        var currentBoard = board.Clone();

        for (int i = 0; i < generations; i++)
        {
            currentBoard = ComputeNextGeneration(currentBoard);
        }

        return currentBoard;
    }

    /// <summary>
    /// Computes the final stable state of the board
    /// </summary>
    /// <param name="board">The current board state</param>
    /// <param name="maxIterations">The maximum number of iterations to compute</param>
    /// <returns>A tuple containing the final board state, whether a stable state was found, and the number of iterations</returns>
    public (Board FinalBoard, bool IsStable, int Iterations, bool IsCyclic, int CycleLength) ComputeFinalState(Board board, int maxIterations = MaxIterations)
    {
        var currentBoard = board.Clone();
        
        // Use a dictionary to store board states and the iteration they were seen
        var history = new Dictionary<string, int>();
        int iteration = 0;

        while (iteration < maxIterations)
        {
            // Check if the current board is empty (all cells are dead)
            if (IsEmptyBoard(currentBoard))
            {
                // If the board is empty, it's stable
                return (currentBoard, true, iteration, false, 0);
            }

            // Compute the next generation
            var nextBoard = ComputeNextGeneration(currentBoard);
            iteration++;

            // Check if the next board is the same as the current board (stable state)
            if (nextBoard.Equals(currentBoard))
            {
                return (nextBoard, true, iteration, false, 0);
            }

            // Create a string representation of the board for efficient comparison
            string boardKey = SerializeBoardState(nextBoard);
            
            // Check if we've seen this board state before (oscillator)
            if (history.TryGetValue(boardKey, out var previousIteration))
            {
                int cycleLength = iteration - previousIteration;
                return (nextBoard, true, iteration, true, cycleLength);
            }

            // Add the current board to the history with its iteration number
            history[SerializeBoardState(currentBoard)] = iteration;
            currentBoard = nextBoard;
        }

        // If we've reached the maximum number of iterations, return the current board
        return (currentBoard, false, iteration, false, 0);
    }
    
    /// <summary>
    /// Checks if a board is empty (all cells are dead)
    /// </summary>
    /// <param name="board">The board to check</param>
    /// <returns>True if the board is empty, false otherwise</returns>
    private bool IsEmptyBoard(Board board)
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (board.Cells[x, y].IsAlive)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Creates a string representation of a board for efficient comparison
    /// </summary>
    /// <param name="board">The board to serialize</param>
    /// <returns>A string representation of the board</returns>
    private string SerializeBoardState(Board board)
    {
        var sb = new System.Text.StringBuilder(board.Width * board.Height);
        
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                sb.Append(board.Cells[x, y].IsAlive ? '1' : '0');
            }
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Counts the number of live neighbors around a cell
    /// </summary>
    /// <param name="board">The board</param>
    /// <param name="x">The x-coordinate of the cell</param>
    /// <param name="y">The y-coordinate of the cell</param>
    /// <returns>The number of live neighbors</returns>
    private int CountLiveNeighbors(Board board, int x, int y)
    {
        int count = 0;

        // Check all 8 neighboring cells
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Skip the cell itself
                if (i == 0 && j == 0)
                {
                    continue;
                }

                // Calculate the coordinates of the neighbor
                int neighborX = x + i;
                int neighborY = y + j;

                // Check if the neighbor is within the bounds of the board
                if (neighborX >= 0 && neighborX < board.Width && neighborY >= 0 && neighborY < board.Height)
                {
                    // Increment the count if the neighbor is alive
                    if (board.Cells[neighborX, neighborY].IsAlive)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }
}