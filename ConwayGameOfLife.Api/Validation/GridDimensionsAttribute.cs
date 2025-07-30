using System.ComponentModel.DataAnnotations;
using ConwayGameOfLife.Api.Models;

namespace ConwayGameOfLife.Api.Validation;

/// <summary>
/// Validates that the grid dimensions match the specified width and height
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class GridDimensionsAttribute : ValidationAttribute
{
    /// <summary>
    /// Validates the grid dimensions
    /// </summary>
    /// <param name="value">The grid value</param>
    /// <param name="validationContext">The validation context</param>
    /// <returns>The validation result</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not bool[,] grid)
        {
            return new ValidationResult("Grid must be a 2D boolean array");
        }

        var boardDto = (BoardDto)validationContext.ObjectInstance;
        
        if (grid.GetLength(0) != boardDto.Width || grid.GetLength(1) != boardDto.Height)
        {
            return new ValidationResult("Grid dimensions do not match the specified width and height");
        }

        return ValidationResult.Success;
    }
}