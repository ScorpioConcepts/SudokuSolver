using System.Collections.Generic;
using System.Windows.Media;
using IntPoint = System.Drawing.Point;

namespace SudokuSolver
{
    public static class SudokuSolve
    {
        //private static int[,] grid;
        public static bool Solved { get; private set; }
        public static MainWindow UIWindow;

        public static void SolvePuzzle(int[,] aGrid)
        {
            Solved = SolvePuzzle(aGrid, 1);
        }

        private static bool SolvePuzzle(int[,] aGrid, int depth)
        {
            while (true)
            {
                // The first thing we need to check is if there are any more -1 in the grid...
                #region Check Unsolved
                bool containsUnsolved = false;
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        if (aGrid[row, col] == -1)
                        {
                            containsUnsolved = true;
                            break;
                        }
                    }
                    if (containsUnsolved == true) break;
                }
                #endregion
                // If no more unsolved blocks, the sudoku is solved
                if (containsUnsolved == false) return true;

                bool numberFound = false;
                // The easiest check is to see if we have any 3x3 block, row or column with only one missing entry.
                // If so, it's easy to fill in
                // First the rows
                #region Row Check
                for (int row = 0; row < 9; row++)
                {
                    int missingEntries = 0;
                    for (int col = 0; col < 9; col++)
                    {
                        if (aGrid[row, col] == -1) missingEntries++;
                    }
                    if (missingEntries == 1)
                    {
                        bool[] number = new bool[10];
                        int missingIdx = -1;
                        for (int col = 0; col < 9; col++)
                        {
                            if (aGrid[row, col] == -1)
                            {
                                missingIdx = col;
                            }
                            else
                            {
                                number[aGrid[row, col]] = true;
                            }
                        }
                        List<int> usedInColumn = FindUsedNumbersByColumn(aGrid, missingIdx);
                        List<int> usedInBlock = FindUsedNumbersByBlock(aGrid, row, missingIdx);
                        for (int num = 1; num < 10; num++)
                        {
                            if (number[num] == false)
                            {
                                if (usedInColumn.Contains(num) == true) return false;
                                if (usedInBlock.Contains(num) == true) return false;

                                aGrid[row, missingIdx] = num;
                                UIWindow.UpdateUI(row, missingIdx, num, Brushes.Green, "Missing in Row", aGrid);
                                numberFound = true;
                                break;
                            }
                        }
                    }
                    if (numberFound == true) break;
                }
                #endregion
                if (numberFound == true) continue;
                // Then the columns
                #region Column Check
                for (int col = 0; col < 9; col++)
                {
                    int missingEntries = 0;
                    for (int row = 0; row < 9; row++)
                    {
                        if (aGrid[row, col] == -1) missingEntries++;
                    }
                    if (missingEntries == 1)
                    {
                        bool[] number = new bool[10];
                        int missingIdx = -1;
                        for (int row = 0; row < 9; row++)
                        {
                            if (aGrid[row, col] == -1)
                            {
                                missingIdx = row;
                            }
                            else
                            {
                                number[aGrid[row, col]] = true;
                            }
                        }
                        List<int> usedInRow = FindUsedNumbersByRow(aGrid, missingIdx);
                        List<int> usedInBlock = FindUsedNumbersByBlock(aGrid, missingIdx, col);
                        for (int num = 1; num < 10; num++)
                        {
                            if (number[num] == false)
                            {
                                if (usedInRow.Contains(num) == true) return false;
                                if (usedInBlock.Contains(num) == true) return false;

                                aGrid[missingIdx, col] = num;
                                UIWindow.UpdateUI(missingIdx, col, num, Brushes.Green, "Missing in Column", aGrid);
                                numberFound = true;
                                break;
                            }
                        }
                    }
                    if (numberFound == true) break;
                }
                #endregion
                if (numberFound == true) continue;
                // Lastly, we need to check the 3x3 blocks
                #region 3x3 Block Check
                for (int startRow = 0; startRow < 9; startRow += 3)
                {
                    for (int startCol = 0; startCol < 9; startCol += 3)
                    {
                        int missingEntries = 0;
                        for (int col = startCol; col < startCol + 3; col++)
                        {
                            for (int row = startRow; row < startRow + 3; row++)
                            {
                                if (aGrid[row, col] == -1) missingEntries++;
                            }
                        }

                        if (missingEntries == 1)
                        {
                            bool[] number = new bool[10];
                            int missingRowIdx = -1, missingColIdx = -1;
                            for (int col = startCol; col < startCol + 3; col++)
                            {
                                for (int row = startRow; row < startRow + 3; row++)
                                {
                                    if (aGrid[row, col] == -1)
                                    {
                                        missingRowIdx = row;
                                        missingColIdx = col;
                                    }
                                    else
                                    {
                                        number[aGrid[row, col]] = true;
                                    }
                                }
                            }
                            List<int> usedInRow = FindUsedNumbersByRow(aGrid, missingRowIdx);
                            List<int> usedInColumn = FindUsedNumbersByColumn(aGrid, missingColIdx);
                            for (int num = 1; num < 10; num++)
                            {
                                if (number[num] == false)
                                {
                                    if (usedInRow.Contains(num) == true) return false;
                                    if (usedInColumn.Contains(num) == true) return false;

                                    aGrid[missingRowIdx, missingColIdx] = num;
                                    UIWindow.UpdateUI(missingRowIdx, missingColIdx, num, Brushes.Green, "Missing in 3x3", aGrid);
                                    numberFound = true;
                                    break;
                                }
                            }
                        }
                        if (numberFound == true) break;
                    }
                    if (numberFound == true) break;
                }
                #endregion
                if (numberFound == true) continue;

                // If we get here, then the simple checks did not find anything
                // So the only thing to do is trying to find a number to insert somewhere that doesn't conflict with any rules
                // but cannot be inserted anywhere else. We will once again do this in the same fashion as the simple checks
                // First by row, then column, then 3x3 block
                #region Row Check
                for (int row = 0; row < 9; row++)
                {
                    List<int> openSlots = FindOpenSlotsByRow(aGrid, row);
                    List<int> usedNumbers = FindUsedNumbersByRow(aGrid, row);
                    foreach (int slot in openSlots)
                    {
                        for (int num = 1; num < 10; num++)
                        {
                            if (usedNumbers.Contains(num) == true) continue;
                            List<int> usedInColumn = FindUsedNumbersByColumn(aGrid, slot);
                            if (usedInColumn.Contains(num) == true) continue;
                            List<int> usedInBlock = FindUsedNumbersByBlock(aGrid, row, slot);
                            if (usedInBlock.Contains(num) == true) continue;

                            bool found = true;
                            foreach (int otherSlot in openSlots)
                            {
                                if (otherSlot == slot) continue;
                                List<int> usedInOtherColumn = FindUsedNumbersByColumn(aGrid, otherSlot);
                                if (usedInOtherColumn.Contains(num) == false)
                                {
                                    found = false;
                                    break;
                                }
                            }

                            if (found == false) continue;

                            aGrid[row, slot] = num;
                            usedNumbers.Add(num);
                            UIWindow.UpdateUI(row, slot, num, Brushes.DarkGoldenrod, "Row Search", aGrid);
                            numberFound = true;
                            break;
                        }
                    }
                }
                #endregion
                if (numberFound == true) continue;
                #region Column Check
                for (int col = 0; col < 9; col++)
                {
                    List<int> openSlots = FindOpenSlotsByColumn(aGrid, col);
                    List<int> usedNumbers = FindUsedNumbersByColumn(aGrid, col);
                    foreach (int slot in openSlots)
                    {
                        for (int num = 1; num < 10; num++)
                        {
                            if (usedNumbers.Contains(num) == true) continue;
                            List<int> usedInRow = FindUsedNumbersByRow(aGrid, slot);
                            if (usedInRow.Contains(num) == true) continue;
                            List<int> usedInBlock = FindUsedNumbersByBlock(aGrid, slot, col);
                            if (usedInBlock.Contains(num) == true) continue;

                            bool found = true;
                            foreach (int otherSlot in openSlots)
                            {
                                if (otherSlot == slot) continue;
                                List<int> usedInOtherColumn = FindUsedNumbersByRow(aGrid, otherSlot);
                                if (usedInOtherColumn.Contains(num) == false)
                                {
                                    found = false;
                                    break;
                                }
                            }

                            if (found == false) continue;

                            aGrid[slot, col] = num;
                            usedNumbers.Add(num);
                            UIWindow.UpdateUI(slot, col, num, Brushes.DarkGoldenrod, "Column Search", aGrid);
                            numberFound = true;
                            break;
                        }
                    }
                }
                #endregion
                if (numberFound == true) continue;
                #region 3x3 Block Check
                for (int row = 0; row < 9; row += 3)
                {
                    for (int col = 0; col < 9; col += 3)
                    {
                        List<IntPoint> openSlots = FindOpenSlotsByBlock(aGrid, row, col);
                        List<int> usedNumbers = FindUsedNumbersByBlock(aGrid, row, col);
                        foreach (IntPoint slot in openSlots)
                        {
                            for (int num = 1; num < 10; num++)
                            {
                                if (usedNumbers.Contains(num) == true) continue;
                                List<int> usedInRow = FindUsedNumbersByRow(aGrid, slot.Y);
                                if (usedInRow.Contains(num) == true) continue;
                                List<int> usedInColumn = FindUsedNumbersByColumn(aGrid, slot.X);
                                if (usedInColumn.Contains(num) == true) continue;

                                bool found = true;
                                foreach (IntPoint otherSlot in openSlots)
                                {
                                    if (otherSlot == slot) continue;
                                    List<int> usedInOtherRow = FindUsedNumbersByRow(aGrid, otherSlot.Y);
                                    List<int> usedInOtherColumn = FindUsedNumbersByColumn(aGrid, otherSlot.X);
                                    if ((usedInOtherRow.Contains(num) == false) && (usedInOtherColumn.Contains(num) == false))
                                    {
                                        found = false;
                                        break;
                                    }
                                }

                                if (found == false) continue;

                                aGrid[slot.Y, slot.X] = num;
                                usedNumbers.Add(num);
                                UIWindow.UpdateUI(slot.Y, slot.X, num, Brushes.OrangeRed, "3x3 Search", aGrid);
                                numberFound = true;
                                break;
                            }
                        }
                    }
                }
                #endregion
                if (numberFound == true) continue;

                //If we get here, then we have exhausted every possible mathematical option, and we need to start making
                // educated guesses and solve the puzzle from there and see if we get a solution.
                //Step 1: Find the most populated 3x3 block with unsolved entries
                List<IntPoint> bestOption = null;
                for (int row = 0; row < 9; row += 3)
                {
                    for (int col = 0; col < 9; col += 3)
                    {
                        List<IntPoint> openSlots = FindOpenSlotsByBlock(aGrid, row, col);
                        if (openSlots.Count < 1) continue;

                        if ((bestOption == null) || (openSlots.Count < bestOption.Count))
                        {
                            bestOption = new List<IntPoint>(openSlots);
                        }
                    }
                }
                {
                    List<int> usedNumbers = FindUsedNumbersByBlock(aGrid, bestOption[0].Y, bestOption[0].X);
                    foreach (IntPoint slot in bestOption)
                    {
                        List<int> usedInRow = FindUsedNumbersByRow(aGrid, slot.Y);
                        List<int> usedInColumn = FindUsedNumbersByColumn(aGrid, slot.X);
                        for (int num = 1; num < 10; num++)
                        {
                            if (usedInRow.Contains(num) == true) continue;
                            if (usedInColumn.Contains(num) == true) continue;
                            if (usedNumbers.Contains(num) == true) continue;
                            int[,] bGrid = CopyGrid(aGrid);

                            bGrid[slot.Y, slot.X] = num;
                            //usedNumbers.Add(num);
                            UIWindow.UpdateUI(slot.Y, slot.X, num, Brushes.Red, "Tree Search", bGrid);

                            bool solved = SolvePuzzle(bGrid, depth + 1);
                            if (solved) return true;
                        }
                    }
                }

                return false;
            }
        }

        private static int[,] CopyGrid(int[,] aGrid)
        {
            int[,] bGrid = new int[9, 9];
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    bGrid[row, col] = aGrid[row, col];
                }
            }

            return bGrid;
        }

        private static List<int> FindOpenSlotsByRow(int[,] grid, int row)
        {
            List<int> openSlots = new List<int>();
            for (int col = 0; col < 9; col++)
            {
                if (grid[row, col] == -1) openSlots.Add(col);
            }

            return openSlots;
        }
        private static List<int> FindOpenSlotsByColumn(int[,] grid, int col)
        {
            List<int> openSlots = new List<int>();
            for (int row = 0; row < 9; row++)
            {
                if (grid[row, col] == -1) openSlots.Add(row);
            }

            return openSlots;
        }
        private static List<IntPoint> FindOpenSlotsByBlock(int[,] grid, int row, int col)
        {
            List<IntPoint> openSlots = new List<IntPoint>();
            int blockX = col / 3;
            int blockY = row / 3;

            for (int y = blockY * 3; y < (blockY * 3) + 3; y++)
            {
                for (int x = blockX * 3; x < (blockX * 3) + 3; x++)
                {
                    if (grid[y, x] == -1) openSlots.Add(new IntPoint(x, y));
                }
            }

            return openSlots;
        }
        private static List<int> FindUsedNumbersByRow(int[,] grid, int row)
        {
            List<int> usedNumbers = new List<int>();
            for (int col = 0; col < 9; col++)
            {
                if (grid[row, col] != -1) usedNumbers.Add(grid[row, col]);
            }

            return usedNumbers;
        }
        private static List<int> FindUsedNumbersByColumn(int[,] grid, int col)
        {
            List<int> usedNumbers = new List<int>();
            for (int row = 0; row < 9; row++)
            {
                if (grid[row, col] != -1) usedNumbers.Add(grid[row, col]);
            }

            return usedNumbers;
        }
        private static List<int> FindUsedNumbersByBlock(int[,] grid, int row, int col)
        {
            List<int> usedNumbers = new List<int>();
            int blockX = col / 3;
            int blockY = row / 3;

            for (int y = blockY * 3; y < (blockY * 3) + 3; y++)
            {
                for (int x = blockX * 3; x < (blockX * 3) + 3; x++)
                {
                    if (grid[y, x] != -1) usedNumbers.Add(grid[y, x]);
                }
            }

            return usedNumbers;
        }
    }
}
