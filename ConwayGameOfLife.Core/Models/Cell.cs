namespace ConwayGameOfLife.Core.Models;

/// <summary>
/// Represents a cell in Conway's Game of Life
/// </summary>
public class Cell
{
    /// <summary>
    /// Gets or sets whether the cell is alive
    /// </summary>
    public bool IsAlive { get; set; }

    /// <summary>
    /// Creates a new cell with the specified state
    /// </summary>
    /// <param name="isAlive">Whether the cell is alive</param>
    public Cell(bool isAlive = false)
    {
        IsAlive = isAlive;
    }

    /// <summary>
    /// Creates a copy of the cell
    /// </summary>
    /// <returns>A new cell with the same state</returns>
    public Cell Clone()
    {
        return new Cell(IsAlive);
    }
}