namespace ConwayGameOfLife.Api.Models;

/// <summary>
/// Data transfer object for a final state response
/// </summary>
public class FinalStateResponseDto
{
    /// <summary>
    /// Gets or sets the board
    /// </summary>
    public BoardResponseDto Board { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the board reached a stable state
    /// </summary>
    public bool IsStable { get; set; }

    /// <summary>
    /// Gets or sets the number of iterations required to reach the final state
    /// </summary>
    public int Iterations { get; set; }
    
    /// <summary>
    /// Gets or sets whether the board is in a cyclic pattern
    /// </summary>
    public bool IsCyclic { get; set; }
    
    /// <summary>
    /// Gets or sets the length of the cycle if the board is cyclic
    /// </summary>
    public int CycleLength { get; set; }
}