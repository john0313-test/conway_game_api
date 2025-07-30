using ConwayGameOfLife.Core.Models;

namespace ConwayGameOfLife.Core.Services;

/// <summary>
/// Interface for Conway's Game of Life service
/// </summary>
public interface IGameOfLifeService
{
    /// <summary>
    /// Computes the next generation of the board
    /// </summary>
    /// <param name="board">The current board state</param>
    /// <returns>The next generation of the board</returns>
    Board ComputeNextGeneration(Board board);

    /// <summary>
    /// Computes the board state after N generations
    /// </summary>
    /// <param name="board">The current board state</param>
    /// <param name="generations">The number of generations to compute</param>
    /// <returns>The board state after N generations</returns>
    Board ComputeGenerations(Board board, int generations);

    /// <summary>
    /// Computes the final stable state of the board
    /// </summary>
    /// <param name="board">The current board state</param>
    /// <param name="maxIterations">The maximum number of iterations to compute</param>
    /// <returns>A tuple containing the final board state, whether a stable state was found, the number of iterations, whether the board is cyclic, and the cycle length</returns>
    (Board FinalBoard, bool IsStable, int Iterations, bool IsCyclic, int CycleLength) ComputeFinalState(Board board, int maxIterations = 1000);
}