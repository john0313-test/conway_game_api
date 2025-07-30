namespace ConwayGameOfLife.Core.Models;

/// <summary>
/// Represents a persisted state of a board in Conway's Game of Life
/// </summary>
public class BoardState
{
    /// <summary>
    /// Gets or sets the unique identifier for the board state
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the width of the board
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the board
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the serialized grid of cells
    /// </summary>
    public string SerializedGrid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the board state was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Creates a new board state
    /// </summary>
    public BoardState()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new board state from a board
    /// </summary>
    /// <param name="board">The board to create the state from</param>
    /// <returns>A new board state</returns>
    public static BoardState FromBoard(Board board)
    {
        var state = new BoardState
        {
            Width = board.Width,
            Height = board.Height,
            SerializedGrid = SerializeGrid(board.ToGrid())
        };

        return state;
    }

    /// <summary>
    /// Converts the board state to a board
    /// </summary>
    /// <returns>A new board with the state's cell configuration</returns>
    public Board ToBoard()
    {
        var grid = DeserializeGrid(SerializedGrid, Width, Height);
        return Board.FromGrid(grid);
    }

    /// <summary>
    /// Serializes a grid of boolean values to a string
    /// </summary>
    /// <param name="grid">The grid to serialize</param>
    /// <returns>A string representation of the grid</returns>
    private static string SerializeGrid(bool[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        var chars = new char[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                chars[y * width + x] = grid[x, y] ? '1' : '0';
            }
        }

        return new string(chars);
    }

    /// <summary>
    /// Deserializes a string to a grid of boolean values
    /// </summary>
    /// <param name="serialized">The serialized grid</param>
    /// <param name="width">The width of the grid</param>
    /// <param name="height">The height of the grid</param>
    /// <returns>A grid of boolean values</returns>
    private static bool[,] DeserializeGrid(string serialized, int width, int height)
    {
        var grid = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < serialized.Length)
                {
                    grid[x, y] = serialized[index] == '1';
                }
            }
        }

        return grid;
    }
}