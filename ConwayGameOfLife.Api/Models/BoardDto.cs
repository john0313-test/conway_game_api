using System.ComponentModel.DataAnnotations;
using ConwayGameOfLife.Api.Validation;

namespace ConwayGameOfLife.Api.Models;

/// <summary>
/// Data transfer object for a board
/// </summary>
public class BoardDto
{
    /// <summary>
    /// Gets or sets the width of the board
    /// </summary>
    [Required]
    [Range(1, 100, ErrorMessage = "Width must be between 1 and 100")]
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the board
    /// </summary>
    [Required]
    [Range(1, 100, ErrorMessage = "Height must be between 1 and 100")]
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the grid of cells
    /// </summary>
    [Required]
    [GridDimensions(ErrorMessage = "Grid dimensions must match the specified width and height")]
    public bool[,] Grid { get; set; } = null!;
}