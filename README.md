# Conway's Game of Life API

A RESTful API for Conway's Game of Life implemented in C# using .NET 8.0.

## Overview

This API allows you to:
- Upload a board state
- Get the next generation of a board
- Get the state of a board after N generations
- Get the final stable state of a board

## Architecture

The solution follows a clean architecture approach with the following projects:

- **ConwayGameOfLife.Api**: Web API project with controllers and DTOs
- **ConwayGameOfLife.Core**: Domain models and business logic
- **ConwayGameOfLife.Infrastructure**: Data access and persistence
- **ConwayGameOfLife.Tests**: Unit tests for all components

## API Endpoints

### Upload Board State
```
POST /api/gameoflife/board
```
Accepts a new board state and returns a unique identifier.

**Request Body:**
```json
{
  "width": 3,
  "height": 3,
  "grid": [
    [false, false, false],
    [false, true, false],
    [false, false, false]
  ]
}
```

**Response:**
```
201 Created
```
```
"3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

### Get Next State
```
GET /api/gameoflife/board/{id}/next
```
Returns the next generation state of the board.

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "width": 3,
  "height": 3,
  "grid": [
    [false, false, false],
    [false, false, false],
    [false, false, false]
  ]
}
```

### Get N States Ahead
```
GET /api/gameoflife/board/{id}/generations/{generations}
```
Returns the board state after N generations.

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "width": 3,
  "height": 3,
  "grid": [
    [false, false, false],
    [false, false, false],
    [false, false, false]
  ]
}
```

### Get Final State
```
GET /api/gameoflife/board/{id}/final?maxIterations=1000
```
Returns the final stable state of the board.

**Response:**
```json
{
  "board": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "width": 3,
    "height": 3,
    "grid": [
      [false, false, false],
      [false, false, false],
      [false, false, false]
    ]
  },
  "isStable": true,
  "iterations": 1
}
```

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- Docker (optional)

### Local Development
```bash
# Clone the repository
git clone https://github.com/yourusername/conway-game-of-life.git
cd conway-game-of-life

# Build and run the API
dotnet build
cd ConwayGameOfLife.Api
dotnet run
```

### Using Docker
```bash
# Build and run with Docker Compose
docker-compose up -d
```

## Running Tests
```bash
dotnet test
```

## Design Decisions

### Persistence
- The application uses Entity Framework Core with SQLite for persistence
- Board states are serialized as strings to optimize storage
- The database is created automatically on startup

### Game Logic
- The game logic is implemented in the `GameOfLifeService` class
- The service follows Conway's Game of Life rules:
  1. Any live cell with fewer than two live neighbors dies (underpopulation)
  2. Any live cell with two or three live neighbors lives on
  3. Any live cell with more than three live neighbors dies (overpopulation)
  4. Any dead cell with exactly three live neighbors becomes a live cell (reproduction)

### Final State Detection
- The application detects both static final states and oscillating patterns
- A maximum iteration limit prevents infinite loops for patterns that don't stabilize

## Production Readiness Features

### Error Handling
- Global exception handling middleware
- Detailed error responses in development, sanitized in production
- Proper HTTP status codes for different error scenarios

### Validation
- Input validation using data annotations
- Custom validation attributes for grid dimensions
- Model validation using ASP.NET Core's built-in validation

### Performance
- Performance benchmarks for core game logic
- Optimized algorithms for board state computation
- Efficient serialization of board states

### Monitoring
- Health check endpoint for monitoring database connectivity
- Detailed health check responses with status and duration
- Ready for integration with monitoring systems

### Security
- Rate limiting middleware to prevent abuse
- Input validation to prevent malicious data
- No sensitive information exposed in responses

### DevOps
- Docker support for containerization
- Docker Compose for local development
- GitHub Actions workflow for CI/CD

## Future Enhancements
- Add caching for frequently accessed board states
- Implement background processing for large boards
- Add visualization capabilities
- Support for different rule sets
- Performance optimizations for large boards
- Add authentication and authorization
- Implement distributed caching
- Add metrics collection and monitoring