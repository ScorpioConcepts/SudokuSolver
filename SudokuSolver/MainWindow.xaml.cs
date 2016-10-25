using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SudokuSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker worker;

        public MainWindow()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            SudokuSolve.UIWindow = this;

#if !DEBUG
            txtLog.Parent.SetValue(ScrollViewer.VisibilityProperty,Visibility.Collapsed);
            this.Width -= 250;
#endif
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            SudokuSolve.SolvePuzzle((int[,])(e.Argument));
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            btnSolve.IsEnabled = false;
            btnSolve.UpdateLayout();
            makeReadOnlyGrid(true);

            int[,] grid = ReadGrid();

            // We have the grid, now let's see if we can solve it
            worker.RunWorkerAsync(grid);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (SudokuSolve.Solved == true)
            {
                MessageBox.Show("Done");
            }
            else
            {
                MessageBox.Show("Cannot Solve");
            }
            makeReadOnlyGrid(false);
            btnSolve.IsEnabled = true;
        }

        private int[,] ReadGrid()
        {
            int[,] newGrid = new int[9, 9];
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    TextBox src = numbers.Children[(row * 9) + col] as TextBox;
                    if (src != null)
                    {
                        if (src.Text.Length > 0)
                        {
                            newGrid[row, col] = int.Parse(src.Text);
                            src.FontWeight = FontWeights.ExtraBold;
                        }
                        else
                        {
                            newGrid[row, col] = -1;
                        }
                    }
                }
            }
            return newGrid;
        }
        private void makeReadOnlyGrid(bool readOnly)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    TextBox src = numbers.Children[(row * 9) + col] as TextBox;
                    if (src != null)
                    {
                        src.IsReadOnly = readOnly;
                    }
                }
            }
        }

        public delegate void UpdateTextBoxDelegate(int row, int col, int num, Brush difficulty, string method, int[,] curGrid);

        public void UpdateUI(int row, int col, int num, Brush difficulty, string method, int[,] curGrid)
        {
            Dispatcher.Invoke(new UpdateTextBoxDelegate(UpdateTextBox), new object[] { row, col, num, difficulty, method, curGrid });
        }
        private void UpdateTextBox(int row, int col, int num, Brush difficulty, string method, int[,] curGrid)
        {
            TextBox src = numbers.Children[(row * 9) + col] as TextBox;
            src.Text = num.ToString();
            src.Foreground = difficulty;

#if DEBUG
            txtLog.Text += Environment.NewLine + "[" + (row + 1) + "," + (col + 1) + "] = " + num + " (" + method + ")";
            Console.WriteLine("[" + (row + 1) + "," + (col + 1) + "] = " + num + " (" + method + ")");
#endif

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (curGrid[y, x] == -1)
                    {
                        src = numbers.Children[(y * 9) + x] as TextBox;
                        src.Text = "";
                    }
                }
            }
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int[,] grid = ReadGrid();

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Sudoku File(*.sudoku)|*.sudoku|All Files|*.*";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dialog.ShowDialog() == true)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Create(dialog.FileName)))
                {
                    for (int y = 0; y < 9; y++)
                    {
                        for (int x = 0; x < 9; x++)
                        {
                            writer.Write(grid[y, x]);
                        }
                    }
                }
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            int[,] grid = new int[9, 9];

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Sudoku File(*.sudoku)|*.sudoku|All Files|*.*";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dialog.ShowDialog() == true)
            {
                clearBoard();
                using (BinaryReader reader = new BinaryReader(File.Open(dialog.FileName, FileMode.Open)))
                {
                    for (int y = 0; y < 9; y++)
                    {
                        for (int x = 0; x < 9; x++)
                        {
                            grid[y, x] = reader.ReadInt32();
                            TextBox src = numbers.Children[(y * 9) + x] as TextBox;
                            if (grid[y, x] > 0)
                            {
                                src.FontWeight = FontWeights.ExtraBold;
                                src.Text = grid[y, x].ToString();
                            }
                            else
                            {
                                src.FontWeight = FontWeights.Normal;
                                src.Text = "";
                            }
                        }
                    }
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            clearBoard();
        }

        private void clearBoard()
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    TextBox src = numbers.Children[(y * 9) + x] as TextBox;
                    clearCell(src);
                }
            }
        }

        private void rowcol_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox src = sender as TextBox;
            int digit;

            if (src.Text.Length != 1) { clearCell(src); return; }
            if (int.TryParse(src.Text, out digit) == false) { clearCell(src); return; }
            if ((digit < 1) || (digit > 9)) { clearCell(src); return; }
            if (validGrid() == false) { src.BorderBrush = Brushes.Red; return; }

            src.FontWeight = FontWeights.ExtraBold;
            src.BorderBrush = Brushes.Gray;
        }

        private void clearCell(TextBox src)
        {
            src.FontWeight = FontWeights.Normal;
            src.Text = "";
            src.Foreground = Brushes.Black;
        }
        private bool validGrid()
        {
            int[,] grid = ReadGrid();
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    int checkVal = grid[y, x];
                    if (checkVal == -1) continue;
                    for( int nx = x+1; nx<9; nx++)
                    {
                        if (grid[y, nx] == checkVal) return false;
                    }
                }
            }
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    int checkVal = grid[y, x];
                    if (checkVal == -1) continue;
                    for (int ny = y + 1; ny < 9; ny++)
                    {
                        if (grid[ny, x] == checkVal) return false;
                    }
                }
            }

            return true;
        }
    }
}
