using System.Text;

namespace ConwayGameOfLife.Core.Models;

/// <summary>
/// Represents a board in Conway's Game of Life
/// </summary>
public class Board
{
    /// <summary>
    /// Gets the width of the board
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the board
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the cells on the board
    /// </summary>
    public Cell[,] Cells { get; }

    /// <summary>
    /// Creates a new board with the specified dimensions
    /// </summary>
    /// <param name="width">The width of the board</param>
    /// <param name="height">The height of the board</param>
    /// <param name="cells">Optional initial cell configuration</param>
    public Board(int width, int height, Cell[,]? cells = null)
    {
        Width = width;
        Height = height;

        if (cells != null && cells.GetLength(0) == width && cells.GetLength(1) == height)
        {
            Cells = cells;
        }
        else
        {
            Cells = new Cell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cells[x, y] = new Cell();
                }
            }
        }
    }

    /// <summary>
    /// Creates a new board from a 2D array of boolean values
    /// </summary>
    /// <param name="grid">The 2D array of boolean values</param>
    /// <returns>A new board with the specified cell states</returns>
    public static Board FromGrid(bool[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        var cells = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new Cell(grid[x, y]);
            }
        }

        return new Board(width, height, cells);
    }

    /// <summary>
    /// Converts the board to a 2D array of boolean values
    /// </summary>
    /// <returns>A 2D array of boolean values representing the cell states</returns>
    public bool[,] ToGrid()
    {
        var grid = new bool[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = Cells[x, y].IsAlive;
            }
        }

        return grid;
    }

    /// <summary>
    /// Creates a deep copy of the board
    /// </summary>
    /// <returns>A new board with the same dimensions and cell states</returns>
    public Board Clone()
    {
        var cells = new Cell[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                cells[x, y] = Cells[x, y].Clone();
            }
        }

        return new Board(Width, Height, cells);
    }

    /// <summary>
    /// Determines whether this board is equal to another board
    /// </summary>
    /// <param name="other">The other board to compare with</param>
    /// <returns>True if the boards have the same dimensions and cell states, false otherwise</returns>
    public bool Equals(Board other)
    {
        if (Width != other.Width || Height != other.Height)
        {
            return false;
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Cells[x, y].IsAlive != other.Cells[x, y].IsAlive)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Computes a hash code for the board
    /// </summary>
    /// <returns>A hash code based on the cell states</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Width);
        hash.Add(Height);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                hash.Add(Cells[x, y].IsAlive);
            }
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Returns a string representation of the board
    /// </summary>
    /// <returns>A string representation of the board</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                sb.Append(Cells[x, y].IsAlive ? "■" : "□");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}