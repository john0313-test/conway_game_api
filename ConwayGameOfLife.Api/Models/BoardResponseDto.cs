namespace ConwayGameOfLife.Api.Models;

/// <summary>
/// Data transfer object for a board response
/// </summary>
public class BoardResponseDto
{
    /// <summary>
    /// Gets or sets the ID of the board
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
    /// Gets or sets the grid of cells
    /// </summary>
    public bool[,] Grid { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the generation number of this board state
    /// </summary>
    public int Generation { get; set; }
}