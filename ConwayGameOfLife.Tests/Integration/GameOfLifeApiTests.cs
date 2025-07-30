using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ConwayGameOfLife.Api.Models;
using ConwayGameOfLife.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConwayGameOfLife.Tests.Integration;

public class GameOfLifeApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GameOfLifeApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's AppDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add AppDbContext using an in-memory database for testing
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task UploadBoardState_ReturnsCreatedWithId()
    {
        // Arrange
        var boardDto = new BoardDto
        {
            Width = 3,
            Height = 3,
            Grid = new bool[3, 3]
            {
                { false, true, false },
                { false, true, false },
                { false, true, false }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(boardDto),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/gameoflife/board", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadAsStringAsync();
        Guid.TryParse(id.Trim('"'), out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetNextState_ReturnsNextGeneration()
    {
        // Arrange
        var boardDto = new BoardDto
        {
            Width = 3,
            Height = 3,
            Grid = new bool[3, 3]
            {
                { false, true, false },
                { false, true, false },
                { false, true, false }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(boardDto),
            Encoding.UTF8,
            "application/json");

        var uploadResponse = await _client.PostAsync("/api/gameoflife/board", content);
        var id = await uploadResponse.Content.ReadAsStringAsync();
        id = id.Trim('"');

        // Act
        var response = await _client.GetAsync($"/api/gameoflife/{id}/next");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var boardResponse = await response.Content.ReadFromJsonAsync<BoardResponseDto>();
        boardResponse.Should().NotBeNull();
        boardResponse!.Width.Should().Be(3);
        boardResponse.Height.Should().Be(3);
        boardResponse.Generation.Should().Be(1);

        // The next generation of a vertical blinker is a horizontal blinker
        boardResponse.Grid[0, 1].Should().BeFalse();
        boardResponse.Grid[1, 0].Should().BeTrue();
        boardResponse.Grid[1, 1].Should().BeTrue();
        boardResponse.Grid[1, 2].Should().BeTrue();
        boardResponse.Grid[2, 1].Should().BeFalse();
    }

    [Fact]
    public async Task GetGenerations_ReturnsCorrectGeneration()
    {
        // Arrange
        var boardDto = new BoardDto
        {
            Width = 3,
            Height = 3,
            Grid = new bool[3, 3]
            {
                { false, true, false },
                { false, true, false },
                { false, true, false }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(boardDto),
            Encoding.UTF8,
            "application/json");

        var uploadResponse = await _client.PostAsync("/api/gameoflife/board", content);
        var id = await uploadResponse.Content.ReadAsStringAsync();
        id = id.Trim('"');

        // Act
        var response = await _client.GetAsync($"/api/gameoflife/{id}/generations/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var boardResponse = await response.Content.ReadFromJsonAsync<BoardResponseDto>();
        boardResponse.Should().NotBeNull();
        boardResponse!.Width.Should().Be(3);
        boardResponse.Height.Should().Be(3);
        boardResponse.Generation.Should().Be(2);

        // After 2 generations, we should be back to a vertical blinker
        boardResponse.Grid[0, 1].Should().BeFalse();
        boardResponse.Grid[1, 0].Should().BeFalse();
        boardResponse.Grid[1, 1].Should().BeTrue();
        boardResponse.Grid[1, 2].Should().BeFalse();
        boardResponse.Grid[2, 1].Should().BeFalse();
    }

    [Fact]
    public async Task GetFinalState_ReturnsFinalState()
    {
        // Arrange
        var boardDto = new BoardDto
        {
            Width = 3,
            Height = 3,
            Grid = new bool[3, 3]
            {
                { false, true, false },
                { false, true, false },
                { false, true, false }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(boardDto),
            Encoding.UTF8,
            "application/json");

        var uploadResponse = await _client.PostAsync("/api/gameoflife/board", content);
        var id = await uploadResponse.Content.ReadAsStringAsync();
        id = id.Trim('"');

        // Act
        var response = await _client.GetAsync($"/api/gameoflife/{id}/final");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalState = await response.Content.ReadFromJsonAsync<FinalStateResponseDto>();
        finalState.Should().NotBeNull();
        finalState!.Board.Should().NotBeNull();
        finalState.IsStable.Should().BeTrue();
        finalState.IsCyclic.Should().BeTrue();
        finalState.CycleLength.Should().Be(2);
    }
}