using ConwayGameOfLife.Core.Models;
using ConwayGameOfLife.Infrastructure.Data;
using ConwayGameOfLife.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConwayGameOfLife.Tests.Infrastructure.Repositories;

public class BoardStateRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    private readonly Mock<ILogger<BoardStateRepository>> _loggerMock;

    public BoardStateRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"ConwayGameOfLifeTest_{Guid.NewGuid()}")
            .Options;

        _loggerMock = new Mock<ILogger<BoardStateRepository>>();
    }

    [Fact]
    public async Task AddAsync_ValidBoardState_ShouldAddAndReturnBoardState()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        var repository = new BoardStateRepository(dbContext, _loggerMock.Object);
        
        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;
        var boardState = BoardState.FromBoard(board);

        // Act
        var result = await repository.AddAsync(boardState);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(boardState.Id);
        result.Width.Should().Be(3);
        result.Height.Should().Be(3);

        // Verify it was added to the database
        var savedState = await dbContext.BoardStates.FindAsync(boardState.Id);
        savedState.Should().NotBeNull();
        savedState!.Width.Should().Be(3);
        savedState.Height.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBoardState_ShouldReturnBoardState()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        var repository = new BoardStateRepository(dbContext, _loggerMock.Object);
        
        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;
        var boardState = BoardState.FromBoard(board);
        
        dbContext.BoardStates.Add(boardState);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(boardState.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(boardState.Id);
        result.Width.Should().Be(3);
        result.Height.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingBoardState_ShouldReturnNull()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        var repository = new BoardStateRepository(dbContext, _loggerMock.Object);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingBoardState_ShouldUpdateAndReturnBoardState()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        var repository = new BoardStateRepository(dbContext, _loggerMock.Object);
        
        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;
        var boardState = BoardState.FromBoard(board);
        
        dbContext.BoardStates.Add(boardState);
        await dbContext.SaveChangesAsync();

        // Update the board state
        var updatedBoard = new Board(4, 4);
        updatedBoard.Cells[2, 2].IsAlive = true;
        var updatedBoardState = BoardState.FromBoard(updatedBoard);
        updatedBoardState.Id = boardState.Id; // Keep the same ID

        // Act
        var result = await repository.UpdateAsync(updatedBoardState);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(boardState.Id);
        result.Width.Should().Be(4);
        result.Height.Should().Be(4);

        // Verify it was updated in the database
        var savedState = await dbContext.BoardStates.FindAsync(boardState.Id);
        savedState.Should().NotBeNull();
        savedState!.Width.Should().Be(4);
        savedState.Height.Should().Be(4);
    }

    [Fact]
    public async Task DeleteAsync_ExistingBoardState_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        var repository = new BoardStateRepository(dbContext, _loggerMock.Object);
        
        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;
        var boardState = BoardState.FromBoard(board);
        
        dbContext.BoardStates.Add(boardState);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.DeleteAsync(boardState.Id);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from the database
        var deletedState = await dbContext.BoardStates.FindAsync(boardState.Id);
        deletedState.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingBoardState_ShouldReturnFalse()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        var repository = new BoardStateRepository(dbContext, _loggerMock.Object);

        // Act
        var result = await repository.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}