using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ConwayGameOfLife.Core.Models;
using ConwayGameOfLife.Core.Services;

namespace ConwayGameOfLife.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class GameOfLifeBenchmarks
{
    private readonly GameOfLifeService _service;
    private Board _smallBoard;
    private Board _mediumBoard;
    private Board _largeBoard;

    public GameOfLifeBenchmarks()
    {
        _service = new GameOfLifeService();
        
        // Initialize boards of different sizes
        _smallBoard = CreateRandomBoard(10, 10);
        _mediumBoard = CreateRandomBoard(50, 50);
        _largeBoard = CreateRandomBoard(100, 100);
    }

    [Benchmark]
    public void ComputeNextGeneration_SmallBoard()
    {
        _service.ComputeNextGeneration(_smallBoard);
    }

    [Benchmark]
    public void ComputeNextGeneration_MediumBoard()
    {
        _service.ComputeNextGeneration(_mediumBoard);
    }

    [Benchmark]
    public void ComputeNextGeneration_LargeBoard()
    {
        _service.ComputeNextGeneration(_largeBoard);
    }

    [Benchmark]
    public void ComputeGenerations_SmallBoard_10Generations()
    {
        _service.ComputeGenerations(_smallBoard, 10);
    }

    [Benchmark]
    public void ComputeGenerations_MediumBoard_10Generations()
    {
        _service.ComputeGenerations(_mediumBoard, 10);
    }

    [Benchmark]
    public void ComputeFinalState_SmallBoard()
    {
        _service.ComputeFinalState(_smallBoard, 100);
    }

    private static Board CreateRandomBoard(int width, int height)
    {
        var board = new Board(width, height);
        var random = new Random(42); // Use a fixed seed for reproducibility
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Approximately 25% of cells will be alive
                board.Cells[x, y].IsAlive = random.Next(4) == 0;
            }
        }
        
        return board;
    }
}