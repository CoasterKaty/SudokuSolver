using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SudokuSolver
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
    class SudokuSquare
    {
        public int Group;
        public int Row;
        public int Col;
        public List<int> PossibleValues = new List<int>();
        private int _value;
        public int Value { 
            set {
                PossibleValues.Clear();
                _value = value; 
            }
            get { 
                return _value;
            } 
        }

        public SudokuSquare(int GroupNum, int RowNum, int ColNum)
        {
            Group = GroupNum;
            Row = RowNum;
            Col = ColNum;
            ResetCell();
            
        }
        public void ResetCell()
        {
            Value = 0;
            PossibleValues = new List<int>();
            for (int i = 1; i <= 9; i++)
            {
                PossibleValues.Add(i);
            }

        }
        
    }
}
