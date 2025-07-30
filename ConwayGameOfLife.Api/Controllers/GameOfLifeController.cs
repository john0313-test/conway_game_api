using ConwayGameOfLife.Api.Models;
using ConwayGameOfLife.Core.Models;
using ConwayGameOfLife.Core.Repositories;
using ConwayGameOfLife.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConwayGameOfLife.Api.Controllers;

/// <summary>
/// Controller for Conway's Game of Life
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GameOfLifeController : ControllerBase
{
    private readonly IGameOfLifeService _gameOfLifeService;
    private readonly IBoardStateRepository _boardStateRepository;
    private readonly ILogger<GameOfLifeController> _logger;

    /// <summary>
    /// Creates a new instance of the Game of Life controller
    /// </summary>
    /// <param name="gameOfLifeService">The Game of Life service</param>
    /// <param name="boardStateRepository">The board state repository</param>
    /// <param name="logger">The logger</param>
    public GameOfLifeController(
        IGameOfLifeService gameOfLifeService,
        IBoardStateRepository boardStateRepository,
        ILogger<GameOfLifeController> logger)
    {
        _gameOfLifeService = gameOfLifeService ?? throw new ArgumentNullException(nameof(gameOfLifeService));
        _boardStateRepository = boardStateRepository ?? throw new ArgumentNullException(nameof(boardStateRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Uploads a new board state
    /// </summary>
    /// <param name="boardDto">The board to upload</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The ID of the uploaded board</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/gameoflife/board
    ///     {
    ///        "width": 3,
    ///        "height": 3,
    ///        "grid": [
    ///          [false, true, false],
    ///          [false, true, false],
    ///          [false, true, false]
    ///        ]
    ///     }
    ///
    /// </remarks>
    [HttpPost("board")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadBoardState([FromBody] BoardDto boardDto, CancellationToken cancellationToken)
    {
        try
        {
            // Manual validation for grid dimensions
            if (boardDto.Grid.GetLength(0) != boardDto.Width || boardDto.Grid.GetLength(1) != boardDto.Height)
            {
                return BadRequest("Grid dimensions do not match the specified width and height");
            }
            
            // Create a board from the DTO
            var board = Board.FromGrid(boardDto.Grid);

            // Create a board state
            var boardState = BoardState.FromBoard(board);

            // Save the board state
            var savedBoardState = await _boardStateRepository.AddAsync(boardState, cancellationToken);

            // Return the ID of the saved board state
            return CreatedAtAction(nameof(GetNextState), new { id = savedBoardState.Id }, savedBoardState.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading board state");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading the board state");
        }
    }

    /// <summary>
    /// Gets the next state of a board
    /// </summary>
    /// <param name="id">The ID of the board</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The next state of the board</returns>
    /// <remarks>
    /// Sample response:
    ///
    ///     {
    ///        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "width": 3,
    ///        "height": 3,
    ///        "grid": [
    ///          [false, false, false],
    ///          [true, true, true],
    ///          [false, false, false]
    ///        ],
    ///        "generation": 1
    ///     }
    ///
    /// </remarks>
    [HttpGet("board/{id}/next")]
    [ProducesResponseType(typeof(BoardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNextState(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            // Get the board state
            var boardState = await _boardStateRepository.GetByIdAsync(id, cancellationToken);
            if (boardState == null)
            {
                return NotFound($"Board with ID {id} not found");
            }

            // Convert to a board
            var board = boardState.ToBoard();

            // Compute the next generation
            var nextBoard = _gameOfLifeService.ComputeNextGeneration(board);

            // Create a response DTO
            var responseDto = new BoardResponseDto
            {
                Id = id,
                Width = nextBoard.Width,
                Height = nextBoard.Height,
                Grid = nextBoard.ToGrid(),
                Generation = 1
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next state for board with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the next state");
        }
    }

    /// <summary>
    /// Gets the state of a board after N generations
    /// </summary>
    /// <param name="id">The ID of the board</param>
    /// <param name="generations">The number of generations to compute</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The state of the board after N generations</returns>
    /// <remarks>
    /// Sample response:
    ///
    ///     {
    ///        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "width": 3,
    ///        "height": 3,
    ///        "grid": [
    ///          [false, true, false],
    ///          [false, true, false],
    ///          [false, true, false]
    ///        ],
    ///        "generation": 2
    ///     }
    ///
    /// </remarks>
    [HttpGet("board/{id}/generations/{generations}")]
    [ProducesResponseType(typeof(BoardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNGenerationsAhead(Guid id, int generations, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the number of generations
            if (generations < 0)
            {
                return BadRequest("Number of generations must be non-negative");
            }

            // Get the board state
            var boardState = await _boardStateRepository.GetByIdAsync(id, cancellationToken);
            if (boardState == null)
            {
                return NotFound($"Board with ID {id} not found");
            }

            // Convert to a board
            var board = boardState.ToBoard();

            // Compute the board state after N generations
            var futureBoard = _gameOfLifeService.ComputeGenerations(board, generations);

            // Create a response DTO
            var responseDto = new BoardResponseDto
            {
                Id = id,
                Width = futureBoard.Width,
                Height = futureBoard.Height,
                Grid = futureBoard.ToGrid(),
                Generation = generations
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting state after {Generations} generations for board with ID {Id}", generations, id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the future state");
        }
    }

    /// <summary>
    /// Gets the final stable state of a board
    /// </summary>
    /// <param name="id">The ID of the board</param>
    /// <param name="maxIterations">The maximum number of iterations to compute</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The final stable state of the board</returns>
    /// <remarks>
    /// Sample response:
    ///
    ///     {
    ///        "board": {
    ///          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///          "width": 3,
    ///          "height": 3,
    ///          "grid": [
    ///            [false, false, false],
    ///            [true, true, true],
    ///            [false, false, false]
    ///          ],
    ///          "generation": 1
    ///        },
    ///        "isStable": true,
    ///        "iterations": 1,
    ///        "isCyclic": true,
    ///        "cycleLength": 2
    ///     }
    ///
    /// </remarks>
    [HttpGet("board/{id}/final")]
    [ProducesResponseType(typeof(FinalStateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinalState(Guid id, [FromQuery] int maxIterations = 1000, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the board state
            var boardState = await _boardStateRepository.GetByIdAsync(id, cancellationToken);
            if (boardState == null)
            {
                return NotFound($"Board with ID {id} not found");
            }

            // Convert to a board
            var board = boardState.ToBoard();

            // Compute the final state
            var (finalBoard, isStable, iterations, isCyclic, cycleLength) = _gameOfLifeService.ComputeFinalState(board, maxIterations);

            // Create a response DTO
            var responseDto = new FinalStateResponseDto
            {
                Board = new BoardResponseDto
                {
                    Id = id,
                    Width = finalBoard.Width,
                    Height = finalBoard.Height,
                    Grid = finalBoard.ToGrid(),
                    Generation = iterations
                },
                IsStable = isStable,
                Iterations = iterations,
                IsCyclic = isCyclic,
                CycleLength = cycleLength
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting final state for board with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the final state");
        }
    }
}