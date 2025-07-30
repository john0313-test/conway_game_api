using ConwayGameOfLife.Core.Models;
using ConwayGameOfLife.Core.Services;
using FluentAssertions;

namespace ConwayGameOfLife.Tests.Core.Services;

public class GameOfLifeServiceTests
{
    private readonly GameOfLifeService _service;

    public GameOfLifeServiceTests()
    {
        _service = new GameOfLifeService();
    }

    [Fact]
    public void ComputeNextGeneration_EmptyBoard_RemainsEmpty()
    {
        // Arrange
        var board = new Board(3, 3);

        // Act
        var nextBoard = _service.ComputeNextGeneration(board);

        // Assert
        for (int x = 0; x < nextBoard.Width; x++)
        {
            for (int y = 0; y < nextBoard.Height; y++)
            {
                nextBoard.Cells[x, y].IsAlive.Should().BeFalse();
            }
        }
    }

    [Fact]
    public void ComputeNextGeneration_SingleLiveCell_Dies()
    {
        // Arrange
        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;

        // Act
        var nextBoard = _service.ComputeNextGeneration(board);

        // Assert
        nextBoard.Cells[1, 1].IsAlive.Should().BeFalse();
    }

    [Fact]
    public void ComputeNextGeneration_Block_RemainsStable()
    {
        // Arrange - Create a 2x2 block pattern
        var board = new Board(4, 4);
        board.Cells[1, 1].IsAlive = true;
        board.Cells[1, 2].IsAlive = true;
        board.Cells[2, 1].IsAlive = true;
        board.Cells[2, 2].IsAlive = true;

        // Act
        var nextBoard = _service.ComputeNextGeneration(board);

        // Assert - The block should remain stable
        nextBoard.Cells[1, 1].IsAlive.Should().BeTrue();
        nextBoard.Cells[1, 2].IsAlive.Should().BeTrue();
        nextBoard.Cells[2, 1].IsAlive.Should().BeTrue();
        nextBoard.Cells[2, 2].IsAlive.Should().BeTrue();
    }

    [Fact]
    public void ComputeNextGeneration_Blinker_Oscillates()
    {
        // Arrange - Create a horizontal blinker pattern
        var board = new Board(5, 5);
        board.Cells[1, 2].IsAlive = true;
        board.Cells[2, 2].IsAlive = true;
        board.Cells[3, 2].IsAlive = true;

        // Act - First generation
        var nextBoard = _service.ComputeNextGeneration(board);

        // Assert - The blinker should oscillate to vertical
        nextBoard.Cells[2, 1].IsAlive.Should().BeTrue();
        nextBoard.Cells[2, 2].IsAlive.Should().BeTrue();
        nextBoard.Cells[2, 3].IsAlive.Should().BeTrue();
        nextBoard.Cells[1, 2].IsAlive.Should().BeFalse();
        nextBoard.Cells[3, 2].IsAlive.Should().BeFalse();

        // Act - Second generation
        var thirdBoard = _service.ComputeNextGeneration(nextBoard);

        // Assert - The blinker should oscillate back to horizontal
        thirdBoard.Cells[1, 2].IsAlive.Should().BeTrue();
        thirdBoard.Cells[2, 2].IsAlive.Should().BeTrue();
        thirdBoard.Cells[3, 2].IsAlive.Should().BeTrue();
        thirdBoard.Cells[2, 1].IsAlive.Should().BeFalse();
        thirdBoard.Cells[2, 3].IsAlive.Should().BeFalse();
    }

    [Fact]
    public void ComputeGenerations_MultipleGenerations_CorrectResult()
    {
        // Arrange - Create a blinker pattern
        var board = new Board(5, 5);
        board.Cells[1, 2].IsAlive = true;
        board.Cells[2, 2].IsAlive = true;
        board.Cells[3, 2].IsAlive = true;

        // Act - Compute 2 generations
        var futureBoard = _service.ComputeGenerations(board, 2);

        // Assert - After 2 generations, the blinker should be back to horizontal
        futureBoard.Cells[1, 2].IsAlive.Should().BeTrue();
        futureBoard.Cells[2, 2].IsAlive.Should().BeTrue();
        futureBoard.Cells[3, 2].IsAlive.Should().BeTrue();
        futureBoard.Cells[2, 1].IsAlive.Should().BeFalse();
        futureBoard.Cells[2, 3].IsAlive.Should().BeFalse();
    }

    [Fact]
    public void ComputeFinalState_Block_ReturnsStableState()
    {
        // Arrange - Create a 2x2 block pattern (already stable)
        var board = new Board(4, 4);
        board.Cells[1, 1].IsAlive = true;
        board.Cells[1, 2].IsAlive = true;
        board.Cells[2, 1].IsAlive = true;
        board.Cells[2, 2].IsAlive = true;

        // Act
        var (finalBoard, isStable, iterations, isCyclic, cycleLength) = _service.ComputeFinalState(board);

        // Assert
        isStable.Should().BeTrue();
        iterations.Should().Be(1); // Takes 1 iteration to detect stability
        finalBoard.Cells[1, 1].IsAlive.Should().BeTrue();
        finalBoard.Cells[1, 2].IsAlive.Should().BeTrue();
        finalBoard.Cells[2, 1].IsAlive.Should().BeTrue();
        finalBoard.Cells[2, 2].IsAlive.Should().BeTrue();
    }

    [Fact]
    public void ComputeFinalState_Blinker_DetectsOscillator()
    {
        // Arrange - Create a horizontal blinker pattern
        var board = new Board(5, 5);
        board.Cells[1, 2].IsAlive = true;
        board.Cells[2, 2].IsAlive = true;
        board.Cells[3, 2].IsAlive = true;

        // Act
        var (finalBoard, isStable, iterations, isCyclic, cycleLength) = _service.ComputeFinalState(board);

        // Assert
        isStable.Should().BeTrue(); // Oscillators are considered stable
        iterations.Should().BeGreaterThan(0); // Should take some iterations to detect
    }

    [Fact]
    public void ComputeFinalState_SingleCell_DetectsExtinction()
    {
        // Arrange - Create a board with a single live cell (will die)
        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;

        // Act
        var (finalBoard, isStable, iterations, isCyclic, cycleLength) = _service.ComputeFinalState(board);

        // Assert
        isStable.Should().BeTrue();
        iterations.Should().Be(1); // Takes 1 iteration to die out
        
        // All cells should be dead
        for (int x = 0; x < finalBoard.Width; x++)
        {
            for (int y = 0; y < finalBoard.Height; y++)
            {
                finalBoard.Cells[x, y].IsAlive.Should().BeFalse();
            }
        }
    }
}