using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// Assuming SudokuLogic.cs is in the same assembly or referenced
// For N, it's public static in SudokuLogic, so SudokuLogic.N can be used.

public class SudokuTests
{
    private readonly SudokuLogic _sudokuLogic;

    public SudokuTests()
    {
        _sudokuLogic = new SudokuLogic();
    }

    private int[,] GetEmptyGrid()
    {
        return new int[SudokuLogic.N, SudokuLogic.N];
    }

    private bool IsGridFull(int[,] grid)
    {
        for (int i = 0; i < SudokuLogic.N; i++)
            for (int j = 0; j < SudokuLogic.N; j++)
                if (grid[i, j] == 0) return false;
        return true;
    }

    private bool IsGridValid(int[,] grid)
    {
        // Check rows
        for (int i = 0; i < SudokuLogic.N; i++)
        {
            var rowVals = new HashSet<int>();
            for (int j = 0; j < SudokuLogic.N; j++)
            {
                if (grid[i, j] == 0) return false; // Must be full for this check
                if (rowVals.Contains(grid[i, j])) return false;
                rowVals.Add(grid[i, j]);
            }
        }
        // Check columns
        for (int j = 0; j < SudokuLogic.N; j++)
        {
            var colVals = new HashSet<int>();
            for (int i = 0; i < SudokuLogic.N; i++)
            {
                if (grid[i, j] == 0) return false;
                if (colVals.Contains(grid[i, j])) return false;
                colVals.Add(grid[i, j]);
            }
        }
        // Check 3x3 boxes
        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxCol = 0; boxCol < 3; boxCol++)
            {
                var boxVals = new HashSet<int>();
                for (int i = boxRow * 3; i < boxRow * 3 + 3; i++)
                {
                    for (int j = boxCol * 3; j < boxCol * 3 + 3; j++)
                    {
                        if (grid[i, j] == 0) return false;
                        if (boxVals.Contains(grid[i, j])) return false;
                        boxVals.Add(grid[i, j]);
                    }
                }
            }
        }
        return true;
    }

    [Fact]
    public void IsSafe_ValidMove_ReturnsTrue()
    {
        int[,] grid = {
            {5,3,0,0,7,0,0,0,0},
            {6,0,0,1,9,5,0,0,0},
            {0,9,8,0,0,0,0,6,0},
            {8,0,0,0,6,0,0,0,3},
            {4,0,0,8,0,3,0,0,1},
            {7,0,0,0,2,0,0,0,6},
            {0,6,0,0,0,0,2,8,0},
            {0,0,0,4,1,9,0,0,5},
            {0,0,0,0,8,0,0,7,9}
        };
        Assert.True(_sudokuLogic.IsSafe(grid, 0, 2, 1)); // Try placing 1 in grid[0,2]
    }

    [Fact]
    public void IsSafe_RowConflict_ReturnsFalse()
    {
        int[,] grid = { { 5, 3, 0, 0, 7, 0, 0, 0, 0 }, {0}, {0}, {0}, {0}, {0}, {0}, {0}, {0} }; // Simplified
        grid[0,2] = 7; // Introduce conflict with grid[0,4] which is 7
        Assert.False(_sudokuLogic.IsSafe(grid, 0, 2, 7));
    }

    [Fact]
    public void IsSafe_ColConflict_ReturnsFalse()
    {
        int[,] grid = { { 5, 3, 0, 0, 7, 0, 0, 0, 0 }, { 6,0,0,1,9,5,0,0,0 }, {0}, {0}, {0}, {0}, {0}, {0}, {0} };
        Assert.False(_sudokuLogic.IsSafe(grid, 2, 0, 5)); // Conflict with grid[0,0]
    }

    [Fact]
    public void IsSafe_BoxConflict_ReturnsFalse()
    {
        int[,] grid = {
            {5,3,0,0,7,0,0,0,0},
            {6,1,0,1,9,5,0,0,0}, // Changed grid[1,1] to 1
            {0,9,8,0,0,0,0,6,0},
            {0}, {0}, {0}, {0}, {0}, {0} // Simplified
        };
        Assert.False(_sudokuLogic.IsSafe(grid, 2, 2, 1)); // Try placing 1 in grid[2,2], box conflict with grid[1,1]
    }

    [Fact]
    public void GenerateFullSolution_CreatesValidAndFullGrid()
    {
        int[,] grid = GetEmptyGrid();
        bool success = _sudokuLogic.GenerateFullSolution(grid, 0, 0);
        Assert.True(success);
        Assert.True(IsGridFull(grid));
        Assert.True(IsGridValid(grid));
    }

    [Fact]
    public void SolveSudokuBacktracking_SolvesEmptyGrid()
    {
        int[,] grid = GetEmptyGrid();
        bool solved = _sudokuLogic.SolveSudokuBacktracking(grid, 0, 0);
        Assert.True(solved);
        Assert.True(IsGridFull(grid));
        Assert.True(IsGridValid(grid));
    }

    [Fact]
    public void SolveSudokuBacktracking_SolvesSimplePuzzleCorrectly()
    {
        int[,] puzzle = {
            {5,3,0,0,7,0,0,0,0},
            {6,0,0,1,9,5,0,0,0},
            {0,9,8,0,0,0,0,6,0},
            {8,0,0,0,6,0,0,0,3},
            {4,0,0,8,0,3,0,0,1},
            {7,0,0,0,2,0,0,0,6},
            {0,6,0,0,0,0,2,8,0},
            {0,0,0,4,1,9,0,0,5},
            {0,0,0,0,8,0,0,7,9}
        };
        // This is a known solvable puzzle.
        // Solution from https://sandiway.arizona.edu/sudoku/examples.html (Example 1)
        int[,] expectedSolution = {
            {5,3,4,6,7,8,9,1,2},
            {6,7,2,1,9,5,3,4,8},
            {1,9,8,3,4,2,5,6,7},
            {8,5,9,7,6,1,4,2,3},
            {4,2,6,8,5,3,7,9,1},
            {7,1,3,9,2,4,8,5,6},
            {9,6,1,5,3,7,2,8,4},
            {2,8,7,4,1,9,6,3,5},
            {3,4,5,2,8,6,1,7,9}
        };

        bool solved = _sudokuLogic.SolveSudokuBacktracking(puzzle, 0, 0);
        Assert.True(solved);
        Assert.Equal(expectedSolution, puzzle);
    }

    [Fact]
    public void BacktrackingWithAC3_SolvesSimplePuzzleCorrectly()
    {
        int[,] puzzle = {
            {5,3,0,0,7,0,0,0,0},
            {6,0,0,1,9,5,0,0,0},
            {0,9,8,0,0,0,0,6,0},
            {8,0,0,0,6,0,0,0,3},
            {4,0,0,8,0,3,0,0,1},
            {7,0,0,0,2,0,0,0,6},
            {0,6,0,0,0,0,2,8,0},
            {0,0,0,4,1,9,0,0,5},
            {0,0,0,0,8,0,0,7,9}
        };
         int[,] expectedSolution = {
            {5,3,4,6,7,8,9,1,2},
            {6,7,2,1,9,5,3,4,8},
            {1,9,8,3,4,2,5,6,7},
            {8,5,9,7,6,1,4,2,3},
            {4,2,6,8,5,3,7,9,1},
            {7,1,3,9,2,4,8,5,6},
            {9,6,1,5,3,7,2,8,4},
            {2,8,7,4,1,9,6,3,5},
            {3,4,5,2,8,6,1,7,9}
        };

        _sudokuLogic.InitializeDomains(puzzle); // Crucial for AC3
        bool solved = _sudokuLogic.BacktrackingWithAC3(puzzle);
        Assert.True(solved);
        Assert.Equal(expectedSolution, puzzle);
    }

    [Fact]
    public void Revise_CorrectlyPrunesDomain()
    {
        // Setup: cellX=(0,0), cellY=(0,1)
        // DomainX = {1,2,3}, DomainY = {1,2}
        // Constraint: valX != valY
        // We expect valX=1 and valX=2 to be removed from DomainX if DomainY only has {1,2}
        // and there is no OTHER value in DomainY that can satisfy the constraint.
        // Example: If DomainX={1,2}, DomainY={1}. Then 2 in Dx is fine. 1 in Dx must be removed.
        // Example: If DomainX={1,2,3}, DomainY={1,2}.
        //   For valX=1 in Dx: valY=2 in Dy makes it consistent. So 1 remains in Dx.
        //   For valX=2 in Dx: valY=1 in Dy makes it consistent. So 2 remains in Dx.
        //   For valX=3 in Dx: valY=1 or valY=2 makes it consistent. So 3 remains in Dx.
        //   No change in this case.

        // Let's try a case where pruning happens:
        // DomainX = {1,2}, DomainY = {1}
        // cellX and cellY are neighbors.
        var testDomains = new Dictionary<(int, int), HashSet<int>>
        {
            { (0,0), new HashSet<int> { 1, 2 } },
            { (0,1), new HashSet<int> { 1 } }
        };
        _sudokuLogic.SetDomainsForTest(testDomains);

        bool revised = _sudokuLogic.Revise((0,0), (0,1));

        Assert.True(revised);
        Assert.Equal(new HashSet<int> { 2 }, _sudokuLogic.GetCurrentDomains()[(0,0)]);
    }

    [Fact]
    public void Revise_NoChangeWhenConsistent()
    {
        var testDomains = new Dictionary<(int, int), HashSet<int>>
        {
            { (0,0), new HashSet<int> { 1, 2, 3 } },
            { (0,1), new HashSet<int> { 1, 2 } }
        };
        _sudokuLogic.SetDomainsForTest(testDomains);

        bool revised = _sudokuLogic.Revise((0,0), (0,1));
        Assert.False(revised); // No value should be removed from domain of (0,0)
        Assert.Equal(new HashSet<int> { 1, 2, 3 }, _sudokuLogic.GetCurrentDomains()[(0,0)]);
    }
}
