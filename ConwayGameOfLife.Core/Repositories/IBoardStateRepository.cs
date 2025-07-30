using ConwayGameOfLife.Core.Models;

namespace ConwayGameOfLife.Core.Repositories;

/// <summary>
/// Repository interface for board states
/// </summary>
public interface IBoardStateRepository
{
    /// <summary>
    /// Gets a board state by its ID
    /// </summary>
    /// <param name="id">The ID of the board state</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The board state, or null if not found</returns>
    Task<BoardState?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new board state
    /// </summary>
    /// <param name="boardState">The board state to add</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The added board state</returns>
    Task<BoardState> AddAsync(BoardState boardState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing board state
    /// </summary>
    /// <param name="boardState">The board state to update</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The updated board state</returns>
    Task<BoardState> UpdateAsync(BoardState boardState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a board state
    /// </summary>
    /// <param name="id">The ID of the board state to delete</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>True if the board state was deleted, false otherwise</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}