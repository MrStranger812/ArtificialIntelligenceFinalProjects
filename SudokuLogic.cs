using System;
using System.Collections.Generic;
using System.Linq;

public class SudokuLogic
{
    public static int N = 9; // Made static for broad accessibility, could be instance if preferred
    private Random random = new Random();
    private Dictionary<(int, int), HashSet<int>> domains;

    public SudokuLogic()
    {
        // Initialize random if it needs a seed or specific setup
    }

    // Made public for testing and use from Razor page
    public bool GenerateFullSolution(int[,] board, int row, int col)
    {
        if (row == N - 1 && col == N)
            return true; // Puzzle solved

        if (col == N)
        {
            row++;
            col = 0;
        }

        if (board[row, col] != 0)
        {
            return GenerateFullSolution(board, row, col + 1);
        }

        var numbers = Enumerable.Range(1, N).OrderBy(x => random.Next()).ToList();

        foreach (int num in numbers)
        {
            if (IsSafe(board, row, col, num))
            {
                board[row, col] = num;

                if (GenerateFullSolution(board, row, col + 1))
                    return true;

                board[row, col] = 0; // Backtrack
            }
        }
        return false;
    }

    // Made public for testing
    public bool IsSafe(int[,] intGrid, int row, int col, int num)
    {
        // Check row
        for (int c = 0; c < N; c++)
        {
            if (intGrid[row, c] == num) return false;
        }

        // Check column
        for (int r = 0; r < N; r++)
        {
            if (intGrid[r, col] == num) return false;
        }

        // Check 3x3 subgrid
        int subgridRow = row - row % 3;
        int subgridCol = col - col % 3;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                if (intGrid[subgridRow + r, subgridCol + c] == num)
                    return false;
            }
        return true;
    }

    // Made public for testing and use from Razor page
    public bool SolveSudokuBacktracking(int[,] intGrid, int row, int col)
    {
        if (row == N - 1 && col == N)
            return true;

        if (col == N)
        {
            row++;
            col = 0;
        }

        if (intGrid[row, col] != 0)
        {
            return SolveSudokuBacktracking(intGrid, row, col + 1);
        }

        for (int num = 1; num <= N; num++) // Standard order for solver
        {
            if (IsSafe(intGrid, row, col, num))
            {
                intGrid[row, col] = num;
                if (SolveSudokuBacktracking(intGrid, row, col + 1))
                    return true;
                intGrid[row, col] = 0; // Backtrack
            }
        }
        return false;
    }

    // Made public for testing and use from Razor page
    public void InitializeDomains(int[,] board)
    {
        domains = new Dictionary<(int, int), HashSet<int>>();
        for (int r = 0; r < N; r++)
        {
            for (int c = 0; c < N; c++)
            {
                if (board[r, c] == 0)
                {
                    var possibleValues = new HashSet<int>(Enumerable.Range(1, 9));
                    // Apply initial constraints based on board
                    // This part was missing in the original InitializeDomains for AC3,
                    // it should respect pre-filled cells for initial domain setup.
                    // However, AC3 usually runs on an empty board's domains for solving.
                    // For now, keeping it as it was, but this is a point of attention.
                    domains[(r, c)] = possibleValues;
                }
                else
                {
                    domains[(r, c)] = new HashSet<int>() { board[r, c] };
                }
            }
        }
    }

    // Made public for testing and use from Razor page
    public bool AC3(int[,] board) // board parameter might be used for initial domain setup consistency
    {
        Queue<(int, int, int, int)> queue = new Queue<(int, int, int, int)>();
        for (int r = 0; r < N; r++)
        {
            for (int c = 0; c < N; c++)
            {
                var neighbors = GetNeighbors(r, c);
                foreach (var (nr, nc) in neighbors)
                    queue.Enqueue((r, c, nr, nc));
            }
        }

        while (queue.Count > 0)
        {
            var (r1, c1, r2, c2) = queue.Dequeue();
            if (Revise((r1, c1), (r2, c2)))
            {
                if (domains[(r1, c1)].Count == 0)
                    return false; // Inconsistent

                var neighbors = GetNeighbors(r1, c1);
                foreach (var (nr, nc) in neighbors)
                    if ((nr, nc) != (r2, c2)) // Do not add the arc (Xk, Xi) if we just processed (Xi, Xk)
                        queue.Enqueue((nr, nc, r1, c1));
            }
        }
        return true;
    }

    // Made public for testing
    public bool Revise((int r, int c) cellX, (int r, int c) cellY)
    {
        bool revised = false;
        var domainX = domains[cellX];
        var domainY = domains[cellY];

        foreach (int valX in domainX.ToList()) // Iterate on a copy
        {
            bool foundConsistentY = false;
            foreach (int valY in domainY)
            {
                if (valX != valY) // Sudoku constraint: neighbors must be different
                {
                    foundConsistentY = true;
                    break;
                }
            }
            if (!foundConsistentY)
            {
                domainX.Remove(valX);
                revised = true;
            }
        }
        return revised;
    }

    // Made public for testing
    public List<(int, int)> GetNeighbors(int row, int col)
    {
        var neighbors = new List<(int, int)>();
        for (int c = 0; c < N; c++)
            if (c != col) neighbors.Add((row, c));
        for (int r = 0; r < N; r++)
            if (r != row) neighbors.Add((r, col));
        int subgridRow = row - row % 3;
        int subgridCol = col - col % 3;
        for (int r_offset = 0; r_offset < 3; r_offset++)
            for (int c_offset = 0; c_offset < 3; c_offset++)
            {
                int rr = subgridRow + r_offset;
                int cc = subgridCol + c_offset;
                if (rr != row || cc != col) // Ensure it's a neighbor, not the cell itself
                    if(!neighbors.Contains((rr,cc))) // Avoid duplicates
                         neighbors.Add((rr, cc));
            }
        return neighbors;
    }

    // Made public for testing and use from Razor page
    public bool BacktrackingWithAC3(int[,] board)
    {
        // InitializeDomains(board); // Domains should be initialized before starting the first call.
        // The current AC3 implementation in Razor calls InitializeDomains outside.
        // For a self-contained class, it might be better to have it here or ensure it's called.

        if (!AC3(board)) // AC3 will use and modify this.domains
            return false;

        var unassigned = domains.Where(d => d.Value.Count > 1).ToList();
        if (unassigned.Count == 0)
        {
            // All cells have a single domain value, apply to board
            foreach (var kvp in domains)
            {
                if (kvp.Value.Count == 1) // Should always be true here if unassigned is empty
                    board[kvp.Key.Item1, kvp.Key.Item2] = kvp.Value.First();
                else if (kvp.Value.Count == 0) return false; // Should have been caught by AC3
            }
            return true; // Solved
        }

        var cellWithMinDomain = unassigned.OrderBy(d => d.Value.Count).First();
        var (row, col) = cellWithMinDomain.Key;

        foreach (int val in cellWithMinDomain.Value.ToList()) // Iterate on a copy
        {
            var originalDomains = CopyDomains(); // Save current state of all domains

            // Try assigning val to cell (row, col)
            domains[(row, col)] = new HashSet<int> { val };
            // board[row,col] = val; // Apply to board tentatively too, or only at the end

            if (BacktrackingWithAC3(board)) // Recursive call
                return true;

            domains = originalDomains; // Backtrack: restore domains
            // board[row,col] = 0; // Backtrack: reset cell on board if it was changed
        }
        return false;
    }

    // Made public for testing
    public Dictionary<(int, int), HashSet<int>> CopyDomains()
    {
        var newDict = new Dictionary<(int, int), HashSet<int>>();
        foreach (var kvp in domains)
            newDict[kvp.Key] = new HashSet<int>(kvp.Value); // Copy HashSet
        return newDict;
    }

    // Getter for domains if needed for tests, or pass to Revise directly
    public Dictionary<(int, int), HashSet<int>> GetCurrentDomains() => domains;
    // Setter for domains if needed for tests for Revise
    public void SetDomainsForTest(Dictionary<(int, int), HashSet<int>> testDomains) => domains = testDomains;
}
