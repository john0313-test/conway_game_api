using ConwayGameOfLife.Api.Controllers;
using ConwayGameOfLife.Api.Models;
using ConwayGameOfLife.Core.Models;
using ConwayGameOfLife.Core.Repositories;
using ConwayGameOfLife.Core.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConwayGameOfLife.Tests.Api.Controllers;

public class GameOfLifeControllerTests
{
    private readonly Mock<IGameOfLifeService> _gameOfLifeServiceMock;
    private readonly Mock<IBoardStateRepository> _boardStateRepositoryMock;
    private readonly Mock<ILogger<GameOfLifeController>> _loggerMock;
    private readonly GameOfLifeController _controller;

    public GameOfLifeControllerTests()
    {
        _gameOfLifeServiceMock = new Mock<IGameOfLifeService>();
        _boardStateRepositoryMock = new Mock<IBoardStateRepository>();
        _loggerMock = new Mock<ILogger<GameOfLifeController>>();
        _controller = new GameOfLifeController(
            _gameOfLifeServiceMock.Object,
            _boardStateRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task UploadBoardState_ValidBoard_ReturnsCreatedWithId()
    {
        // Arrange
        var boardDto = new BoardDto
        {
            Width = 3,
            Height = 3,
            Grid = new bool[3, 3]
        };
        boardDto.Grid[1, 1] = true;

        var boardState = new BoardState
        {
            Id = Guid.NewGuid(),
            Width = 3,
            Height = 3,
            SerializedGrid = "000100000" // 3x3 grid with center cell alive
        };

        _boardStateRepositoryMock.Setup(repo => repo.AddAsync(
                It.IsAny<BoardState>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardState);

        // Act
        var result = await _controller.UploadBoardState(boardDto, CancellationToken.None);

        // Assert
        var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(GameOfLifeController.GetNextState));
        createdAtActionResult.RouteValues!["id"].Should().Be(boardState.Id);
        createdAtActionResult.Value.Should().Be(boardState.Id);
    }

    [Fact]
    public async Task UploadBoardState_InvalidDimensions_ReturnsBadRequest()
    {
        // Arrange
        var boardDto = new BoardDto
        {
            Width = 3,
            Height = 3,
            Grid = new bool[4, 4] // Dimensions don't match
        };

        // Act
        var result = await _controller.UploadBoardState(boardDto, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Grid dimensions do not match the specified width and height");
    }

    [Fact]
    public async Task GetNextState_ExistingBoard_ReturnsNextState()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState
        {
            Id = boardId,
            Width = 3,
            Height = 3,
            SerializedGrid = "000100000" // 3x3 grid with center cell alive
        };

        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;

        var nextBoard = new Board(3, 3);
        // In the next generation, the single cell dies

        _boardStateRepositoryMock.Setup(repo => repo.GetByIdAsync(
                boardId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardState);

        _gameOfLifeServiceMock.Setup(service => service.ComputeNextGeneration(
                It.IsAny<Board>()))
            .Returns(nextBoard);

        // Act
        var result = await _controller.GetNextState(boardId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseDto = okResult.Value.Should().BeOfType<BoardResponseDto>().Subject;
        responseDto.Id.Should().Be(boardId);
        responseDto.Width.Should().Be(3);
        responseDto.Height.Should().Be(3);
        // All cells should be dead in the next generation
        for (int x = 0; x < responseDto.Width; x++)
        {
            for (int y = 0; y < responseDto.Height; y++)
            {
                responseDto.Grid[x, y].Should().BeFalse();
            }
        }
    }

    [Fact]
    public async Task GetNextState_NonExistingBoard_ReturnsNotFound()
    {
        // Arrange
        var boardId = Guid.NewGuid();

        _boardStateRepositoryMock.Setup(repo => repo.GetByIdAsync(
                boardId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardState?)null);

        // Act
        var result = await _controller.GetNextState(boardId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Board with ID {boardId} not found");
    }

    [Fact]
    public async Task GetNGenerationsAhead_ValidRequest_ReturnsFutureState()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var generations = 5;
        var boardState = new BoardState
        {
            Id = boardId,
            Width = 3,
            Height = 3,
            SerializedGrid = "000100000" // 3x3 grid with center cell alive
        };

        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;

        var futureBoard = new Board(3, 3);
        // After 5 generations, the board is empty

        _boardStateRepositoryMock.Setup(repo => repo.GetByIdAsync(
                boardId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardState);

        _gameOfLifeServiceMock.Setup(service => service.ComputeGenerations(
                It.IsAny<Board>(),
                generations))
            .Returns(futureBoard);

        // Act
        var result = await _controller.GetNGenerationsAhead(boardId, generations, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseDto = okResult.Value.Should().BeOfType<BoardResponseDto>().Subject;
        responseDto.Id.Should().Be(boardId);
        responseDto.Width.Should().Be(3);
        responseDto.Height.Should().Be(3);
        // All cells should be dead after 5 generations
        for (int x = 0; x < responseDto.Width; x++)
        {
            for (int y = 0; y < responseDto.Height; y++)
            {
                responseDto.Grid[x, y].Should().BeFalse();
            }
        }
    }

    [Fact]
    public async Task GetNGenerationsAhead_NegativeGenerations_ReturnsBadRequest()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var generations = -1;

        // Act
        var result = await _controller.GetNGenerationsAhead(boardId, generations, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Number of generations must be non-negative");
    }

    [Fact]
    public async Task GetFinalState_ValidRequest_ReturnsFinalState()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var boardState = new BoardState
        {
            Id = boardId,
            Width = 3,
            Height = 3,
            SerializedGrid = "000100000" // 3x3 grid with center cell alive
        };

        var board = new Board(3, 3);
        board.Cells[1, 1].IsAlive = true;

        var finalBoard = new Board(3, 3);
        // Final state is an empty board
        bool isStable = true;
        int iterations = 1;

        _boardStateRepositoryMock.Setup(repo => repo.GetByIdAsync(
                boardId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardState);

        _gameOfLifeServiceMock.Setup(service => service.ComputeFinalState(
                It.IsAny<Board>(),
                It.IsAny<int>()))
            .Returns((finalBoard, isStable, iterations, false, 0));

        // Act
        var result = await _controller.GetFinalState(boardId, 1000, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseDto = okResult.Value.Should().BeOfType<FinalStateResponseDto>().Subject;
        responseDto.Board.Id.Should().Be(boardId);
        responseDto.Board.Width.Should().Be(3);
        responseDto.Board.Height.Should().Be(3);
        responseDto.IsStable.Should().BeTrue();
        responseDto.Iterations.Should().Be(1);
        // All cells should be dead in the final state
        for (int x = 0; x < responseDto.Board.Width; x++)
        {
            for (int y = 0; y < responseDto.Board.Height; y++)
            {
                responseDto.Board.Grid[x, y].Should().BeFalse();
            }
        }
    }

    [Fact]
    public async Task GetFinalState_NonExistingBoard_ReturnsNotFound()
    {
        // Arrange
        var boardId = Guid.NewGuid();

        _boardStateRepositoryMock.Setup(repo => repo.GetByIdAsync(
                boardId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardState?)null);

        // Act
        var result = await _controller.GetFinalState(boardId, 1000, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Board with ID {boardId} not found");
    }
}