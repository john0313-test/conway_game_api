using ConwayGameOfLife.Core.Models;
using ConwayGameOfLife.Core.Repositories;
using ConwayGameOfLife.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConwayGameOfLife.Infrastructure.Repositories;

/// <summary>
/// Repository for board states
/// </summary>
public class BoardStateRepository : IBoardStateRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BoardStateRepository> _logger;

    /// <summary>
    /// Creates a new instance of the board state repository
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="logger">The logger</param>
    public BoardStateRepository(AppDbContext dbContext, ILogger<BoardStateRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a board state by its ID
    /// </summary>
    /// <param name="id">The ID of the board state</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The board state, or null if not found</returns>
    public async Task<BoardState?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.BoardStates
                .AsNoTracking()
                .FirstOrDefaultAsync(bs => bs.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting board state with ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Adds a new board state
    /// </summary>
    /// <param name="boardState">The board state to add</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The added board state</returns>
    public async Task<BoardState> AddAsync(BoardState boardState, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbContext.BoardStates.Add(boardState);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return boardState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding board state with ID {Id}", boardState.Id);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing board state
    /// </summary>
    /// <param name="boardState">The board state to update</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The updated board state</returns>
    public async Task<BoardState> UpdateAsync(BoardState boardState, CancellationToken cancellationToken = default)
    {
        try
        {
            // First check if the entity is already being tracked
            var existingEntity = await _dbContext.BoardStates.FindAsync(new object[] { boardState.Id }, cancellationToken);
            if (existingEntity != null)
            {
                // Detach the existing entity to avoid tracking conflicts
                _dbContext.Entry(existingEntity).State = EntityState.Detached;
            }
            
            // Now attach and mark as modified
            _dbContext.Entry(boardState).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return boardState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating board state with ID {Id}", boardState.Id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a board state
    /// </summary>
    /// <param name="id">The ID of the board state to delete</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>True if the board state was deleted, false otherwise</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var boardState = await _dbContext.BoardStates.FindAsync(new object[] { id }, cancellationToken);
            if (boardState == null)
            {
                return false;
            }

            _dbContext.BoardStates.Remove(boardState);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting board state with ID {Id}", id);
            throw;
        }
    }
}