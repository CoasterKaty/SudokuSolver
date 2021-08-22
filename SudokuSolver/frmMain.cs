using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SudokuSolver
{
    public partial class frmMain : Form
    {
        public Font fCell = new Font("Segoe UI", 10);
        public Font fNotes = new Font("Segoe UI", 7);
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            foreach (Control cControl in pnlSudoku.Controls)
            {
                if (cControl.GetType().ToString() == "System.Windows.Forms.Panel") { 
                    foreach (TextBox tCell in cControl.Controls)
                    {
                        int intGroup = Convert.ToInt32(cControl.Tag.ToString());
                        int intRow = 0;
                        int intCol = 0;
                        int intColDiff = 1;
                        int intRowDiff = 1;
                        switch (intGroup)
                        {
                            case 1:
                            case 4: 
                            case 7:
                                intColDiff = 0;
                                break;
                            case 2:
                            case 5:
                            case 8:
                                intColDiff = 3;
                                break;
                            case 3:
                            case 6:
                            case 9:
                                intColDiff = 6;
                                break;
                        }
                        switch (intGroup)
                        {
                            case 1:
                            case 2:
                            case 3:
                                intRowDiff = 0;
                                break;
                            case 4:
                            case 5:
                            case 6:
                                intRowDiff = 3;
                                break;
                            case 7:
                            case 8:
                            case 9:
                                intRowDiff = 6;
                                break;
                        }
                        switch (Convert.ToInt32(tCell.Tag.ToString()))
                        {
                            case 1:
                            case 4:
                            case 7:
                                intCol = 1;
                                break;
                            case 2:
                            case 5:
                            case 8:
                                intCol = 2;
                                break;
                            case 3:
                            case 6:
                            case 9:
                                intCol = 3;
                                break;
                        }
                        switch (Convert.ToInt32(tCell.Tag.ToString()))
                        {
                            case 1:
                            case 2:
                            case 3:
                                intRow = 1;
                                break;
                            case 4:
                            case 5:
                            case 6:
                                intRow = 2;
                                break;
                            case 7:
                            case 8:
                            case 9:
                                intRow = 3;
                                break;
                        }
                        intRow = intRow + intRowDiff;
                        intCol = intCol + intColDiff;
                        tCell.Tag = new SudokuSquare(intGroup, intRow, intCol);
                    }
                }
            }
            btnReset_Click(sender, e);

        }

        private void r1c1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSolve_Click(object sender, EventArgs e)
        {
            int intCellsRemaining = 81;
            //First process completed cells
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell in cControl.Controls)
                {
                    if (tCell.TextLength > 0)
                    {
                        ((SudokuSquare)tCell.Tag).Value = Convert.ToInt32(tCell.Text);
                        intCellsRemaining--;
                    }
                }
            }

            int intIterations = 0;
            int intDoublePass = 0;
            bool bolChanges;
            do
            {
                intIterations++;
                bolChanges = false;
                //Now try each grid filling in possible numbers
                foreach (Control cControl in pnlSudoku.Controls)
                {
                    //For each grid 1-9
                    foreach (TextBox tCell in cControl.Controls)
                    {
                        //For each cell 1-9
                        // if no value, possible values are (1-9) excluding anything in this grid, row and column
                        SudokuSquare ssCell = ((SudokuSquare)tCell.Tag);

                        if (ssCell.PossibleValues.Count > 0) { tCell.Text = ""; }
                        
                        if (ssCell.Value == 0)
                        {
                            List<int> NewPossibles = new List<int>();
                            foreach (int Target in ssCell.PossibleValues)
                            {
                                if (!checkGroup(ssCell.Group, Target) && !checkRow(ssCell.Row, Target) && !checkCol(ssCell.Col, Target))
                                {
                                    NewPossibles.Add(Target);
                                    tCell.Font = fNotes;
                                    tCell.ForeColor = Color.Red;
                                    tCell.AppendText(Target.ToString());
                                }
                            }
                            if (NewPossibles.Count != ssCell.PossibleValues.Count)
                            {
                                ssCell.PossibleValues = NewPossibles;
                                if (ssCell.PossibleValues.Count == 1)
                                {
                                    ssCell.Value = ssCell.PossibleValues[0];
                                    SolvedCell(tCell, ssCell);
                                    Application.DoEvents();
                                    intCellsRemaining--;
                                }
                                bolChanges = true;
                            }
                        }
                    }
                }
                if (!bolChanges)
                {
                    //Check through each grid, then row, then column for "is there only one candidate for {x in 1-9}"
                    foreach (Control cControl in pnlSudoku.Controls)
                    {
                        foreach (TextBox tCell in cControl.Controls)
                        {
                            SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                            if (ssCell.PossibleValues.Count > 1)
                            {
                                //More than 1 possible value for this cell, so see if its values exist in the grid.
                                foreach (int Target in ssCell.PossibleValues)
                                {
                                    if (!checkGroupPossibles(ssCell.Group, Target, tCell.Handle))
                                    {
                                        ssCell.Value = Target;
                                        SolvedCell(tCell, ssCell);
                                        Application.DoEvents();
                                        bolChanges = true;
                                        intCellsRemaining--;
                                        break;
                                    }
                                    if (!checkRowPossibles(ssCell.Row, Target, tCell.Handle))
                                    {
                                        ssCell.Value = Target;
                                        SolvedCell(tCell, ssCell);
                                        Application.DoEvents();
                                        bolChanges = true;
                                        intCellsRemaining--;
                                        break;
                                    }
                                    if (!checkColPossibles(ssCell.Col, Target, tCell.Handle))
                                    {
                                        ssCell.Value = Target;
                                        SolvedCell(tCell, ssCell);
                                        Application.DoEvents();
                                        bolChanges = true;
                                        intCellsRemaining--;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!bolChanges)
                {
                    // Need to check each group/row/col for pairs and exclude them from other cells.... somehow
                    foreach (Control cControl in pnlSudoku.Controls)
                    {
                        txtOutput.AppendText("---Checking Group for doubles: " + cControl.Tag.ToString() + "\r\n");
                        foreach (TextBox tCell in cControl.Controls)
                        {
                            SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                            if (ssCell.PossibleValues.Count == 2)
                            {
                                List<int> lPossibles = ssCell.PossibleValues;
                                List<IntPtr> Handles = new List<IntPtr>();
                                List<int> Col = new List<int>();
                                List<int> Row = new List<int>();
                                List<int> Group = new List<int>();

                                //look for matching pairs
                                txtOutput.AppendText("----Possible double:" + string.Join(",", ssCell.PossibleValues.ToArray()) + "\r\n");

                                //In the same grid
                                Handles.Add(tCell.Handle);
                                Row.Add(ssCell.Row);
                                Col.Add(ssCell.Col);
                                Group.Add(ssCell.Group);
                                foreach (TextBox tCell2 in cControl.Controls)
                                {
                                    SudokuSquare ssCell2 = (SudokuSquare)tCell2.Tag;
                                    if (tCell2.Handle != tCell.Handle)
                                    {
                                        if (ssCell2.PossibleValues.Count == 2)
                                        {
                                            //Is this the same as the first double?
                                            if (Enumerable.SequenceEqual(lPossibles, ssCell2.PossibleValues))
                                            // if (ssCell.PossibleValues == ssCell2.PossibleValues)
                                            {
                                                Row.Add(ssCell2.Row);
                                                Col.Add(ssCell2.Col);
                                                Group.Add(ssCell.Group);
                                                Handles.Add(tCell2.Handle);
                                                //yes, look for these numbers in other squares to exclude them
                                                int cellsChanged = ExcludeDoubles(Group, Row, Col, lPossibles, Handles);
                                                if (cellsChanged > 0)
                                                {
                                                    intCellsRemaining = intCellsRemaining - cellsChanged;
                                                    bolChanges = true;
                                                }
                                                Application.DoEvents();
                                                break;
                                            }
                                        }
                                    }
                                }

                                Row.Clear();
                                Col.Clear();
                                Group.Clear();
                                Handles.Clear();
                                Handles.Add(tCell.Handle);
                                Row.Add(ssCell.Row);
                                Col.Add(ssCell.Col);
                                Group.Add(ssCell.Group);
                                //If it's in the same row
                                foreach (Control cControlR in pnlSudoku.Controls)
                                {
                                    foreach (TextBox tCell2 in cControlR.Controls)
                                    {
                                        SudokuSquare ssCell2 = (SudokuSquare)tCell2.Tag;
                                        if (tCell2.Handle != tCell.Handle && ssCell2.Row == ssCell.Row)
                                        {
                                            if (ssCell2.PossibleValues.Count == 2)
                                            {
                                                //Is this the same as the first double?
                                                if (Enumerable.SequenceEqual(lPossibles, ssCell2.PossibleValues))
                                                // if (ssCell.PossibleValues == ssCell2.PossibleValues)
                                                {
                                                    Row.Add(ssCell2.Row);
                                                    Col.Add(ssCell2.Col);
                                                    Group.Add(ssCell2.Group);
                                                    Handles.Add(tCell2.Handle);
                                                    //yes, look for these numbers in other squares to exclude them
                                                    int cellsChanged = ExcludeDoubles(Group, Row, Col, lPossibles, Handles);

                                                    if (cellsChanged > 0)
                                                    {
                                                        intCellsRemaining = intCellsRemaining - cellsChanged;
                                                        bolChanges = true;
                                                    }
                                                    Application.DoEvents();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                
                                Row.Clear();
                                Col.Clear();
                                Group.Clear();
                                Handles.Clear();
                                Handles.Add(tCell.Handle);
                                Row.Add(ssCell.Row);
                                Col.Add(ssCell.Col);
                                Group.Add(ssCell.Group);
                                //If it's in the same col
                                foreach (Control cControlR in pnlSudoku.Controls)
                                {
                                    foreach (TextBox tCell2 in cControlR.Controls)
                                    {
                                        SudokuSquare ssCell2 = (SudokuSquare)tCell2.Tag;
                                        if (tCell2.Handle != tCell.Handle && ssCell2.Col == ssCell.Col)
                                        {
                                            if (ssCell2.PossibleValues.Count == 2)
                                            {
                                                //Is this the same as the first double?
                                                if (Enumerable.SequenceEqual(lPossibles, ssCell2.PossibleValues))
                                                // if (ssCell.PossibleValues == ssCell2.PossibleValues)
                                                {
                                                    Row.Add(ssCell2.Row);
                                                    Col.Add(ssCell2.Col);
                                                    Group.Add(ssCell2.Group);
                                                    Handles.Add(tCell2.Handle);
                                                    //yes, look for these numbers in other squares to exclude them
                                                    int cellsChanged = ExcludeDoubles(Group, Row, Col, lPossibles, Handles);

                                                    if (cellsChanged > 0)
                                                    {
                                                        intCellsRemaining = intCellsRemaining - cellsChanged;
                                                        bolChanges = true;
                                                    }
                                                    Application.DoEvents();
                                                    break;
                                                }
                                            }
                                        }

                                    }
                                }
                                    
                                Application.DoEvents();
                            }
                        }
                    }
                    intDoublePass++;
                }

                

                if (!bolChanges)
                {
                    // If there's 3 cells in one group, e.g. 68 678 678 then eliminate these numbers from the rest of the group

                    foreach (Control cControl in pnlSudoku.Controls) {
                        
                        foreach (TextBox tCell in cControl.Controls)
                        {
                            SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                            if (ssCell.PossibleValues.Count == 3)
                            {
                                //Find a matching 2nd cell
                                foreach (TextBox tCell2 in cControl.Controls)
                                {
                                    SudokuSquare ssCell2 = (SudokuSquare)tCell2.Tag;
                                    if (ssCell2.PossibleValues.Count == 3 && tCell2.Handle != tCell.Handle && Enumerable.SequenceEqual(ssCell.PossibleValues, ssCell2.PossibleValues))
                                    {
                                        //Find a 3rd match or partial match
                                        foreach (TextBox tCell3 in cControl.Controls)
                                        {
                                            SudokuSquare ssCell3 = (SudokuSquare)tCell3.Tag;
                                            if ((ssCell3.PossibleValues.Count == 3 || ssCell3.PossibleValues.Count == 2) && tCell3.Handle != tCell.Handle && tCell3.Handle != tCell2.Handle)
                                            {
                                                bool bolMatch = false;
                                                //Is it the same or partial?
                                                if ((ssCell3.PossibleValues.Count == 3 && Enumerable.SequenceEqual(ssCell.PossibleValues, ssCell3.PossibleValues)))
                                                {
                                                    bolMatch = true;
                                                }

                                                if ((ssCell3.PossibleValues.Count == 2))
                                                {
                                                    bolMatch = true;
                                                    //If it's partial, i.e. are 67 within 678
                                                    foreach (int Item in ssCell3.PossibleValues)
                                                    {
                                                        if (!ssCell.PossibleValues.Contains(Item))
                                                        {
                                                            bolMatch = false;
                                                            break;
                                                        }
                                                    }
                                                    
                                                }
                                                    
                                                if (bolMatch) 
                                                {
                                                    txtOutput.AppendText("Thing for " + string.Join("", ssCell.PossibleValues.ToArray()) + ", " + string.Join("", ssCell2.PossibleValues.ToArray()) + ", " + string.Join("", ssCell3.PossibleValues.ToArray()) + " has happened\r\n");
                                                    //Exclude this from other cells in the group
                                                    foreach (TextBox tCell4 in cControl.Controls)
                                                    {
                                                        SudokuSquare ssCell4 = (SudokuSquare)tCell4.Tag;
                                                        if (ssCell4.PossibleValues.Count > 1)
                                                        {
                                                            if (tCell4.Handle != tCell3.Handle && tCell4.Handle != tCell2.Handle && tCell4.Handle != tCell.Handle)
                                                            {
                                                                foreach (int Item in ssCell.PossibleValues)
                                                                {


                                                                    if (ssCell4.PossibleValues.Contains(Item))
                                                                    {
                                                                        ssCell4.PossibleValues.Remove(Item);
                                                                        tCell4.Text = string.Join("", ssCell4.PossibleValues.ToArray());
                                                                        bolChanges = true;
                                                                        Application.DoEvents();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                if (intDoublePass ==1) { bolChanges = true; }
                if (intDoublePass == 2) { intDoublePass = 0; }
  
                lblCellsRemaining.Text = intCellsRemaining.ToString() + " cells remaining";
            } while ((intCellsRemaining > 0 && intIterations < 1000) && bolChanges);
        
            if (intCellsRemaining == 0)
            {
               // txtOutput.AppendText("Iterations: " + intIterations.ToString());
            }
            else
            {

                txtOutput.AppendText("I'm Stuck!");

               

            }

        }

       
        private int ExcludeDoubles(List<int> Group, List<int> Row, List<int> Col, List<int>Possibles, List<IntPtr>Handles)
        {
            int RetVal = 0;
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell3 in cControl.Controls)
                {
                    //if the group, row or col matches
                    SudokuSquare ssCell3 = (SudokuSquare)tCell3.Tag;
                    // if it's the same group, or all the Row entries are the same, or all the Col entries are the same
                    if (CheckList(Group, ssCell3.Group) || (CheckList(Row, ssCell3.Row) || CheckList(Col, ssCell3.Col)))
                    {


                        if (!Handles.Contains(tCell3.Handle))
                        {
                            foreach (int intPossVal in Possibles)
                            {
                                if (ssCell3.PossibleValues.Contains(intPossVal))
                                {
                                    ssCell3.PossibleValues.Remove(intPossVal);
                                    tCell3.Text = string.Join("", ssCell3.PossibleValues.ToArray());
                                    txtOutput.AppendText("----Eliminated " + intPossVal.ToString() + " from G" + ssCell3.Group.ToString() + "R" + ssCell3.Row.ToString() + "C" + ssCell3.Col.ToString() + "\r\n");
                                    if (ssCell3.PossibleValues.Count == 1)
                                    {
                                        
                                        ssCell3.Value = (ssCell3.PossibleValues.ToArray())[0];
                                        SolvedCell(tCell3, ssCell3);
                                        RetVal++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return RetVal;
        }

        private bool CheckList(List<int> theList, int theVal)
        {
            foreach (int Item in theList)
            {
                if (Item != theVal)
                {
                    return false;
                }
            }
            return true;
        }
        private void SolvedCell(TextBox tCell, SudokuSquare ssCell)
        {
            if (ssCell.Group == 6 && ssCell.Col == 8 && ssCell.Row == 4)
            {
    //            txtOutput.AppendText("Here");
            }
            txtOutput.AppendText("!!!Solved G" + ssCell.Group.ToString() + "R" + ssCell.Row.ToString() + "C" + ssCell.Col.ToString() + " = " + ssCell.Value.ToString() + "\r\n");
            tCell.Font = fCell;
            tCell.Text = ssCell.Value.ToString();
            tCell.ForeColor = Color.Blue;
            Application.DoEvents();
        }
        private bool checkGroupPossibles(int Group, int Target, IntPtr Caller)
        {
            // Look in all cells in grid's possible values for target
            foreach (Control cControl in pnlSudoku.Controls)
            {
                if (Convert.ToInt32(cControl.Tag.ToString()) == Group)
                {

                    foreach (TextBox tCell in cControl.Controls)
                    {
                        if (tCell.Handle != Caller)
                        {
                            SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                            if (ssCell.PossibleValues.Contains(Target) || ssCell.Value == Target)
                            {
                                return true;
                            }
                        }

                    }
                }

            }
            return false;
        }
       
        private bool checkGroup(int Group, int Target)
        {
            // Look in all cells in grid for target
            foreach (Control cControl in pnlSudoku.Controls)
            {
                if (Convert.ToInt32(cControl.Tag.ToString()) == Group)
                {

                    foreach (TextBox tCell in cControl.Controls)
                    {
                        if (((SudokuSquare)tCell.Tag).Value == Target)
                        {
                            return true;
                        }

                    }
                }
            
            }
            return false;
        }

        
        private bool checkRow(int Row, int Target)
        {
            // Look in all cells in row for target
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell in cControl.Controls)
                {
                    if (((SudokuSquare)tCell.Tag).Row == Row)
                    {
                        if (((SudokuSquare)tCell.Tag).Value == Target)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private bool checkRowPossibles(int Row, int Target, IntPtr Caller)
        {
            // Look in all cells in row for target
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell in cControl.Controls)
                {
                    if (tCell.Handle != Caller)
                    {
                        SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                        if (ssCell.Row == Row)
                        {
                            if (ssCell.PossibleValues.Contains(Target) || ssCell.Value == Target)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private bool checkColPossibles(int Col, int Target, IntPtr Caller)
        {
            // Look in all cells in row for target
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell in cControl.Controls)
                {
                    if (tCell.Handle != Caller)
                    {
                        SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                        if (ssCell.Col == Col)
                        {
                            if (ssCell.PossibleValues.Contains(Target) || ssCell.Value == Target)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool checkCol(int Col, int Target)
        {
            // Look in all cells in col for target
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell in cControl.Controls)
                {
                    if (((SudokuSquare)tCell.Tag).Col == Col)
                    {
                        if (((SudokuSquare)tCell.Tag).Value == Target)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtOutput.Text = "";
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell in cControl.Controls)
                {
                    SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                    ssCell.ResetCell();
                    if (sender.GetType().ToString() == "System.Windows.Forms.ToolStripMenuItem" || tCell.ForeColor != Color.Black)
                    {
                        tCell.Text = "";
                        tCell.Font = fCell;
                        tCell.ForeColor = Color.Black;
                        tCell.TextAlign = HorizontalAlignment.Center;
                        tCell.Multiline = true;
                        tCell.MinimumSize = new Size(30, 30);
                        tCell.Multiline = false;
                    }
                }
            }
            this.Text = "Katy's Sudoku Solver";
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                btnReset_Click(sender, e);
                string strFilePath = dlgOpenFile.FileName;
                int intLine = 0;

                var fileStream = dlgOpenFile.OpenFile();
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    do
                    {
                        intLine++;
                        int intCol = 0;
                        string strLine = reader.ReadLine();
                        foreach (string strCell in strLine.Split(','))
                        {
                            intCol++;
                            if (strCell.Length == 1) fillCell(intLine, intCol, Convert.ToInt32(strCell));
                        }
                    } while (!reader.EndOfStream);
                }
                this.Text = "Katy's Sudoku Solver - " + strFilePath;
                txtOutput.AppendText("\r\n\r\n==Opened File " + strFilePath + "\r\n\r\n");

            }
        }

        private void fillCell(int Row, int Col, int Value)
        {
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell in cControl.Controls)
                {
                    SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                    if (ssCell.Row == Row && ssCell.Col == Col)
                    {
                        ssCell.Value = Value;
                        tCell.Text = Value.ToString();
                    }
                }
            }

        }

        private string ReadCell(int Row, int Col)
        {
            foreach (Control cControl in pnlSudoku.Controls)
            {
                foreach (TextBox tCell in cControl.Controls)
                {
                    SudokuSquare ssCell = (SudokuSquare)tCell.Tag;
                    if (ssCell.Row == Row && ssCell.Col == Col)
                    {
                        return tCell.Text;
                    }
                }
            }
            return "";

        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dlgSave.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter outputFile = new StreamWriter(dlgSave.FileName))
                {
                    for (int i = 1; i <= 9; i++)
                    {
                        string thisRow = "";
                        for (int x = 1; x <= 9; x++)
                        {
                            thisRow += "," + ReadCell(i, x);
                        }
                        outputFile.WriteLine(thisRow.Substring(1));
                    }

                }
                this.Text = "Katy's Sudoku Solver - " + dlgSave.FileName;

            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnReset_Click(sender, e);
        }
    }
}
