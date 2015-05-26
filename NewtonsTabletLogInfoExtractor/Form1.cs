using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NewtonsTabletLogInfoExtractor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<string> listArrayOfText;
        List<string> listArrayOfFinalDataReadyToBeWrittenIntoFile; // This is used to save the output from Post-process.
        string[] listOfFilesToOpen;
        
        string studentID;

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = true;
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();            
        }

        private void button2_Click(object sender, EventArgs e)
        {   
            listArrayOfText = new List<string>();
            listArrayOfFinalDataReadyToBeWrittenIntoFile = new List<string>();
            listOfFilesToOpen = openFileDialog1.FileNames;
            string[] phaseInfoFromASingleFile;
            string lastKnownDirectory = "";
            System.IO.FileInfo fInfo;
            foreach(string singleFile in listOfFilesToOpen)
            {                
                fInfo = new System.IO.FileInfo(singleFile);                
                studentID = fInfo.Directory.Name;
                lastKnownDirectory = System.IO.Path.GetDirectoryName(singleFile);
                // Don't forget the data array below ALREADY has the file name information stored in data[0].
                string[] data = grabTextFileIntoArray(singleFile);
                phaseInfoFromASingleFile = grabPhaseInfoFromSingleFile(data);
                listArrayOfFinalDataReadyToBeWrittenIntoFile.AddRange(phaseInfoFromASingleFile);
            }
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(lastKnownDirectory + "\\Temp.txt"))
            {
                for (int i = 0; i <= listArrayOfFinalDataReadyToBeWrittenIntoFile.Count() - 1; i++)
                {
                    sw.WriteLine(listArrayOfFinalDataReadyToBeWrittenIntoFile[i]);
                }                
            }
            postProcessingForMiddleFile(listArrayOfFinalDataReadyToBeWrittenIntoFile.ToArray());                           
        }

        // As a convenience, we add the filename at the start of the array.
        private string[] grabTextFileIntoArray(string fileNameWithLocation)
        { 
            string[] outputArray;
            using(System.IO.StreamReader sr = new System.IO.StreamReader(fileNameWithLocation))
            {
                // Add the file name at the start of the list array.
                string fileNameWithExtension = System.IO.Path.GetFileName(fileNameWithLocation);
                
                listArrayOfText.Add(fileNameWithExtension);
                while(!sr.EndOfStream)
                {
                    // Save the text file into a list array.
                    string data = sr.ReadLine();
                    listArrayOfText.Add(data);
                }                
            }
            outputArray = listArrayOfText.ToArray();
            // Clear the array after usage.
            listArrayOfText = new List<string>();
            
            return outputArray;            
        }

        private string[] grabPhaseInfoFromSingleFile(string[] stringArrayOfText)
        {
            List<string> phaseInfoFromASingleFile = new List<string>();
            bool isUserAtForceDrawingStage = false;

            phaseInfoFromASingleFile.Add("===================START OF ANALYSIS===================\n");
            phaseInfoFromASingleFile.Add("Student ID: " + studentID + "\n");
            phaseInfoFromASingleFile.Add("File name: " + stringArrayOfText[0] + "\n");
            
            for (int i = 1; i <= stringArrayOfText.Count() - 1; i++)
            {
                string lineToBeChecked = stringArrayOfText[i];
                
                // Check which problem is being solved.
                if (lineToBeChecked.StartsWith("INFORMATION;MENU_ITEM_CLICK;singleBodyProblem"))
                {
                    isUserAtForceDrawingStage = false;
                    // Break the string into a char array and check of the 46th element is a number.
                    char[] characters = lineToBeChecked.ToCharArray();
                    int num;
                    bool isNumber = int.TryParse(characters[45].ToString(), out num);
                    if (isNumber)
                        phaseInfoFromASingleFile.Add(lineToBeChecked + "\n");
                }
                //// Check if boundary trace is completed.
                //else if (lineToBeChecked.StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_2,COLOR=#FF10F02D,Successfully recognized your boundary trace;"))
                //{
                //    textBox2.AppendText("BOUNDARY TRACE COMPLETED." + "\n");
                //}
                //// Check if POIs are located.
                //else if (lineToBeChecked.StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_1,COLOR=#FFCCCCFF,Classify points of interaction;")
                //            && stringArrayOfText[i + 1].StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_2,COLOR=#FF10F02D,That's correct"))
                //{
                //    textBox2.AppendText("LOCATE POIs COMPLETED." + "\n");
                //}
                //// Check if types of POIs are identified.
                //else if (lineToBeChecked.StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_1,COLOR=#FFCCCCFF,Draw forces"))
                //{
                //    textBox2.AppendText("IDENTIFY POI TYPE COMPLETED." + "\n");
                //}
                //// Check if forces are drawn.
                //else if (lineToBeChecked.StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_1,COLOR=#FFCCCCFF,Enter equation type")
                //            && stringArrayOfText[i+1].StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_2,COLOR=#FF10F02D,That's correct"))
                //{
                //    textBox2.AppendText("DRAW FORCE COMPLETED." + "\n");
                //}
                
                // Grab every detail regarding the equation boxes.

                else if (lineToBeChecked.StartsWith("INFORMATION;BUTTON_CLICK;prevEQNButton") |
                         lineToBeChecked.StartsWith("INFORMATION;BUTTON_CLICK;nextEQNButton") |
                         lineToBeChecked.StartsWith("INFORMATION;BUTTON_CLICK;newEQNButton") |
                            lineToBeChecked.StartsWith("INFORMATION;TEXT_CHANGE;eqnTypeTextBox") | 
                            lineToBeChecked.StartsWith("INFORMATION;TEXT_CHANGE;forceExpTermSignTextBox") |
                            lineToBeChecked.StartsWith("INFORMATION;TEXT_CHANGE;forceExpTermTextBox") |
                            lineToBeChecked.StartsWith("INFORMATION;TEXT_CHANGE;momentSymTermSignTextBox") |
                            lineToBeChecked.StartsWith("INFORMATION;TEXT_CHANGE;momentSymTermForceTextBox") |
                            lineToBeChecked.StartsWith("INFORMATION;TEXT_CHANGE;momentExpTermForceTextBox") |
                            lineToBeChecked.StartsWith("INFORMATION;TEXT_CHANGE;momentExpTermMomentArmTextBox"))
                {
                    phaseInfoFromASingleFile.Add(lineToBeChecked + "\n");
                }               

                else if (lineToBeChecked.StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_1,COLOR=#FFCCCCFF,Equation complete"))
                {
                    phaseInfoFromASingleFile.Add(lineToBeChecked + "\n");
                }

                else if (lineToBeChecked.StartsWith("INFORMATION;STROKE_CREATE") && isUserAtForceDrawingStage == true)
                {
                    phaseInfoFromASingleFile.Add("INFORMATION;STROKE_CREATE" + "\n");
                    isUserAtForceDrawingStage = false;
                }

                else if(lineToBeChecked.StartsWith("INFORMATION;STAGE_SKIP;transitioned from POI_CLASSIFICATION to FORCE_DRAWING"))
                {
                    isUserAtForceDrawingStage = true;
                    phaseInfoFromASingleFile.Add(lineToBeChecked + "\n");
                }

                else if (lineToBeChecked.StartsWith("INFORMATION;STAGE_SKIP;transitioned from FORCE_DRAWING to EQN_TYPE_ENTRY") |
                        lineToBeChecked.StartsWith("INFORMATION;STAGE_SKIP;transitioned from EQN_TYPE_ENTRY to FORCE_IDENTIFICATION") |
                        lineToBeChecked.StartsWith("INFORMATION;STAGE_SKIP;transitioned from EQN_TYPE_ENTRY to MOMENT_DIRECTION_DRAWING") |
                        lineToBeChecked.StartsWith("INFORMATION;STAGE_SKIP;transitioned from EXPANDED_TERM_ENTRY to EQUATION_COMPLETE"))
                {
                    phaseInfoFromASingleFile.Add(lineToBeChecked + "\n");
                }

                // Finding the clue if the user had indeed completed the moment equation.
                //else if (lineToBeChecked.StartsWith("INFORMATION;TEXT_CHANGE;eqnTypeTextBox1: M"))
                //{
                //    int j = i + 1;
                //    bool hadFoundConfirmationOfMomentEQInsert = false;
                //    bool hadFoundEquationCompleteMessage = false;
                //    bool hadFoundSignOfStartingNewProblem = false;
                //    bool isEqnTypeTextBox2XorY = false;
                //    while (hadFoundEquationCompleteMessage == false && j <= stringArrayOfText.Count() - 1)
                //    {
                //        // If we can find a log message showing a new problem has been chosen BEFORE "Equation complete" message is found,
                //        // then that means the user has not finished the equation for the current problem.
                //        if (stringArrayOfText[j].StartsWith("INFORMATION;MENU_ITEM_CLICK;singleBodyProblem"))
                //        {
                //            hadFoundSignOfStartingNewProblem = true;
                //            break;
                //        }

                //        // Make sure the second textbox of equation is not X nor Y.
                //        else if (stringArrayOfText[j].StartsWith("INFORMATION;TEXT_CHANGE;eqnTypeTextBox2: X"))
                //        {
                //            isEqnTypeTextBox2XorY = true;
                //        }
                //        else if (stringArrayOfText[j].StartsWith("INFORMATION;TEXT_CHANGE;eqnTypeTextBox2: Y"))
                //        {
                //            isEqnTypeTextBox2XorY = true;
                //        }

                //        else if (stringArrayOfText[j].StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_1,COLOR=#FFCCCCFF,Choose the positive direction"))
                //        {
                //            hadFoundConfirmationOfMomentEQInsert = true;
                //        }

                //        else if (stringArrayOfText[j].StartsWith("INFORMATION;STATUS_BAR_CHANGED;STATUS_BAR_1,COLOR=#FFCCCCFF,Equation complete")
                //            && hadFoundSignOfStartingNewProblem == false && hadFoundConfirmationOfMomentEQInsert == true
                //            && isEqnTypeTextBox2XorY == false)
                //        {
                //            hadFoundEquationCompleteMessage = true;
                //            textBox2.AppendText("MOMENT EQUATION COMPLETED" + "\n");
                //            break;
                //        }
                //        j++;
                //    }
                //}
            }
            phaseInfoFromASingleFile.Add("====================END OF ANALYSIS====================\n");
            return phaseInfoFromASingleFile.ToArray();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = "";
            foreach (string file in openFileDialog1.FileNames)
            {
                textBox1.AppendText(file+";");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog1.FileName = "SUMMARY_" + studentID;
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            using(System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog1.OpenFile()))
            {
                sw.Write("Student ID: " + studentID + "\n" + textBox2.Text);
            }
            // MessageBox.Show("Your file has been saved.", "Notice");
        }

        private string translateProblemCodeNameIntoReadableForm(string currentProblemCodeName)
        {
            string problemNameInReadableForm = "";
            switch(currentProblemCodeName)
            {
                case "singleBodyProblem0":
                    problemNameInReadableForm = "L-Beam";
                    break;
                case "singleBodyProblem1":
                    problemNameInReadableForm = "Bolted";
                    break;
                case "singleBodyProblem3":
                    problemNameInReadableForm = "Truck";
                    break;
            }
            return problemNameInReadableForm;
        }


        // After grabbing the necessary information from the original log file,
        // we now try to find all the elements that the user types in the equation box
        // and try to concatenate them back together again.
        private void postProcessingForMiddleFile(string[] arrayOfMiddleFile)
        {
            string currentProblem;

            bool isUserAtForceDrawingStage = false;

            string[] eqnTypeTextBox = new string[4];
            string[] forceExpTermSignTextBox = new string[10];
            string[] forceExpTermTextBox = new string[10];
            string[] momentSymTermSignTextBox = new string[10];
            string[] momentExpTermMomentArmTextBox = new string[10];
            string[] momentExpTermForceTextBox = new string[10];

            List<FourBoxEquation> listArrayOfFourBoxEquation = new List<FourBoxEquation>();
            int indexOfEquationPreCompleting = 0;


            for (int i = 0; i <= arrayOfMiddleFile.Count()-1; i++)
            {
                if (arrayOfMiddleFile[i].Contains("OF ANALYSIS") | arrayOfMiddleFile[i].Contains("Student ID:") |
                    arrayOfMiddleFile[i].Contains("File name:"))
                {
                    // textBox2.AppendText(arrayOfMiddleFile[i] + "\n");
                }

                // When we encounter a message showing "singleBodyProblem", that means the user has selected a new problem to solve.
                // Hence, clear out everything that was stored before in eqnTypeTextBox, forceExpTermSignTextBox etc.
                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;MENU_ITEM_CLICK;singleBodyProblem"))
                {
                    string[] individualWords = arrayOfMiddleFile[i].Split(';');
                    currentProblem = translateProblemCodeNameIntoReadableForm(individualWords[2]);
                    textBox2.AppendText("The user is solving: " + currentProblem + "\n");
                    
                    isUserAtForceDrawingStage = false;
                    listArrayOfFourBoxEquation = new List<FourBoxEquation>();
                    indexOfEquationPreCompleting = 0;

                    eqnTypeTextBox = new string[4];
                    forceExpTermSignTextBox = new string[10];
                    forceExpTermTextBox = new string[10];
                    momentSymTermSignTextBox = new string[10];
                    
                    momentExpTermMomentArmTextBox = new string[10];
                    momentExpTermForceTextBox = new string[10];
                }

                // Save whatever is in eqnTypeTextBox when stage is skipped to FORCE_IDENTIFICATION. 
                // Then advance the indexOfEquationPreComputing by one.
                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;STAGE_SKIP;transitioned from EQN_TYPE_ENTRY to FORCE_IDENTIFICATION"))
                {
                    listArrayOfFourBoxEquation[indexOfEquationPreCompleting].equationType = eqnTypeTextBox[0];
                    listArrayOfFourBoxEquation[indexOfEquationPreCompleting].relevantAxisOrAnchorPoint = eqnTypeTextBox[1];
                    listArrayOfFourBoxEquation[indexOfEquationPreCompleting].equalSignBox = eqnTypeTextBox[2];
                    listArrayOfFourBoxEquation[indexOfEquationPreCompleting].summationResult = eqnTypeTextBox[3];
                    textBox2.AppendText(eqnTypeTextBox[1] + " EQUATION ATTEMPTED." + "\n");
                }

                // You see this message when the user completed "MA=0" (or M?=0) but had not advanced to the symbolic and expansion term.
                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;STAGE_SKIP;transitioned from EQN_TYPE_ENTRY to MOMENT_DIRECTION_DRAWING"))
                {
                    listArrayOfFourBoxEquation[indexOfEquationPreCompleting].equationType = eqnTypeTextBox[0];
                    listArrayOfFourBoxEquation[indexOfEquationPreCompleting].relevantAxisOrAnchorPoint = eqnTypeTextBox[1];
                    listArrayOfFourBoxEquation[indexOfEquationPreCompleting].equalSignBox = eqnTypeTextBox[2];
                    listArrayOfFourBoxEquation[indexOfEquationPreCompleting].summationResult = eqnTypeTextBox[3];
                    textBox2.AppendText("MOMENT EQUATION ATTEMPTED." + "\n");
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;BUTTON_CLICK;newEQNButton"))
                {
                    listArrayOfFourBoxEquation.Add(new FourBoxEquation());
                    indexOfEquationPreCompleting++;
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;BUTTON_CLICK;prevEQNButton"))
                {
                    indexOfEquationPreCompleting--;
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;BUTTON_CLICK;nextEQNButton"))
                {
                    indexOfEquationPreCompleting++;
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;TEXT_CHANGE;eqnTypeTextBox"))
                {
                    char[] characters = arrayOfMiddleFile[i].ToCharArray();
                    int num;
                    bool isNumber = int.TryParse(characters[38].ToString(), out num);
                    if (isNumber)
                    {
                        // If the 38+1th character is an integer, convert it and use it as an index to store the information
                        // inside characters[41] into eqnTypeTextBox, which is the character that the user inserted in the equation.
                        int eqnTypeTextBoxIndex = Convert.ToInt16(characters[38].ToString());
                        eqnTypeTextBox[eqnTypeTextBoxIndex - 1] = characters[41].ToString();                        
                    }
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;TEXT_CHANGE;forceExpTermSignTextBox"))
                {
                    char[] characters = arrayOfMiddleFile[i].ToCharArray();
                    int num;
                    bool isNumber = int.TryParse(characters[48].ToString(), out num);
                    if (isNumber)
                    {
                        // If the 48+1th character is an integer, convert it and use it as an index to store the information
                        // inside characters[51] into eqnTypeTextBox, which is the character that the user inserted in the equation.
                        int forceExpTermSignTextBoxIndex = Convert.ToInt16(characters[48].ToString());
                        forceExpTermSignTextBox[forceExpTermSignTextBoxIndex - 1] = characters[51].ToString();
                    }
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;TEXT_CHANGE;forceExpTermTextBox"))
                {
                    char[] characters = arrayOfMiddleFile[i].ToCharArray();
                    int num;
                    bool isNumber = int.TryParse(characters[44].ToString(), out num);
                    if (isNumber)
                    {
                        int forceExpTermTextBoxIndex = Convert.ToInt16(characters[44].ToString());
                        // Grab only the substring sandwiched between " " and ";".
                        int first = arrayOfMiddleFile[i].IndexOf(" ") + " ".Length;
                        int last = arrayOfMiddleFile[i].LastIndexOf(";");
                        string midString = arrayOfMiddleFile[i].Substring(first, last - first);
                        forceExpTermTextBox[forceExpTermTextBoxIndex - 1] = midString;
                    }
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;TEXT_CHANGE;momentSymTermSignTextBox"))
                {
                    char[] characters = arrayOfMiddleFile[i].ToCharArray();
                    int num;
                    bool isNumber = int.TryParse(characters[49].ToString(), out num);
                    if (isNumber)
                    {
                        int momentSymTermSignTextBoxIndex = Convert.ToInt16(characters[49].ToString());
                        momentSymTermSignTextBox[momentSymTermSignTextBoxIndex-1]=characters[52].ToString();
                    }
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;TEXT_CHANGE;momentExpTermForceTextBox"))
                {
                    char[] characters = arrayOfMiddleFile[i].ToCharArray();
                    int num;
                    bool isNumber = int.TryParse(characters[50].ToString(), out num);
                    if (isNumber)
                    {
                        int momentExpTermForceTextBoxIndex = Convert.ToInt16(characters[50].ToString());
                        int first = arrayOfMiddleFile[i].IndexOf(" ") + " ".Length;
                        int last = arrayOfMiddleFile[i].LastIndexOf(";");
                        string midString = arrayOfMiddleFile[i].Substring(first, last - first);
                        momentExpTermForceTextBox[momentExpTermForceTextBoxIndex - 1] = midString;
                    }
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;TEXT_CHANGE;momentExpTermMomentArmTextBox"))
                {
                    char[] characters = arrayOfMiddleFile[i].ToCharArray();
                    int num;
                    bool isNumber = int.TryParse(characters[54].ToString(), out num);
                    if (isNumber)
                    {
                        int momentExpTermMomentArmTextBoxIndex = Convert.ToInt16(characters[54].ToString());
                        int first = arrayOfMiddleFile[i].IndexOf(" ") + " ".Length;
                        int last = arrayOfMiddleFile[i].LastIndexOf(";");
                        string midString = arrayOfMiddleFile[i].Substring(first, last - first);
                        momentExpTermMomentArmTextBox[momentExpTermMomentArmTextBoxIndex - 1] = midString;
                    }
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;STAGE_SKIP;transitioned from POI_CLASSIFICATION to FORCE_DRAWING"))
                {
                    isUserAtForceDrawingStage = true;
                }

                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;STROKE_CREATE") && isUserAtForceDrawingStage == true)
                {
                    textBox2.AppendText("FBD ATTEMPTED." + "\n");
                    isUserAtForceDrawingStage = false;
                }

                // If the stage proceeds to EQN_TYPE_ENTRY, that means the student completed FBD drawing.
                // Then we add a space to the listArrayOfFourBoxEquation.
                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;STAGE_SKIP;transitioned from FORCE_DRAWING to EQN_TYPE_ENTRY"))
                {
                    isUserAtForceDrawingStage = false;
                    listArrayOfFourBoxEquation.Add(new FourBoxEquation());
                    textBox2.AppendText("FBD COMPLETED." + "\n");
                }

                // The clear sign showing the student successfully completed a equation to the expanded term, i.e. completed all.
                if (arrayOfMiddleFile[i].StartsWith("INFORMATION;STAGE_SKIP;transitioned from EXPANDED_TERM_ENTRY to EQUATION_COMPLETE"))
                {
                    textBox2.AppendText("Equation: " + listArrayOfFourBoxEquation[indexOfEquationPreCompleting].equationType
                                                        + listArrayOfFourBoxEquation[indexOfEquationPreCompleting].relevantAxisOrAnchorPoint
                                                        + listArrayOfFourBoxEquation[indexOfEquationPreCompleting].equalSignBox
                                                        + listArrayOfFourBoxEquation[indexOfEquationPreCompleting].summationResult + "\n");

                    // See if this is a force equation.
                    if (eqnTypeTextBox[0].Equals("F"))
                    {
                        int counter = 0;
                        for (int j = 0; j <= forceExpTermSignTextBox.Count() - 1;j++)
                        {
                            if (forceExpTermSignTextBox[j]!=null)
                                counter++;
                        }
                        for (int k = 0; k <= counter - 1;k++)
                        {
                            textBox2.AppendText(forceExpTermSignTextBox[k]+forceExpTermTextBox[k]);
                        }
                        textBox2.AppendText("=0" + "\n");
                    }
                    
                    // See if this is a moment equation.
                    else if (eqnTypeTextBox[0].Equals("M"))
                    {
                        int counter = 0;
                        for (int j = 0; j <= momentSymTermSignTextBox.Count()-1;j++)
                        {
                            if (momentSymTermSignTextBox[j]!=null)
                                counter++;
                        }
                        for (int k = 0; k <= counter - 1;k++)
                        {
                            textBox2.AppendText(momentSymTermSignTextBox[k]+"("+momentExpTermForceTextBox[k]+")"
                                +"("+momentExpTermMomentArmTextBox[k]+")");
                        }
                        textBox2.AppendText("=0"+"\n");
                    }
                    eqnTypeTextBox = new string[4];
                    forceExpTermSignTextBox = new string[10];
                    forceExpTermTextBox = new string[10];
                    momentSymTermSignTextBox = new string[10];
                    momentExpTermMomentArmTextBox = new string[10];
                    momentExpTermForceTextBox = new string[10];
                }

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {            
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
                textBox3.Text = folderBrowserDialog1.SelectedPath;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
                textBox4.Enabled = true;
            else
                textBox4.Enabled = false;
        }
        
        List<string> nameOfStaticsProblem;
        
        private void button5_Click(object sender, EventArgs e)
        {
            nameOfStaticsProblem = new List<string>();
            string[] dirs = System.IO.Directory.GetFiles(textBox3.Text);
            MessageBox.Show("Size of dirs: "+ dirs.Count(), "Notice");
            string[] giantArrayOfDataToBeGraded = concatenateAllTextFileIntoOneArray(dirs);
            // MessageBox.Show("Size of giantArray: " + giantArrayOfDataToBeGraded.Count(), "Notice");
            string[] s = textBox5.Text.Split(',');
            for (int i = 0; i <= s.Count() - 1;i++ )
            {
                nameOfStaticsProblem.Add(s[i]);
            }
            int numberOfProblemToBeGraded = s.Count();
            Student[] listOfStudent = autoGradeTheSummaryFiles(giantArrayOfDataToBeGraded, numberOfProblemToBeGraded);
            // MessageBox.Show("Size of listOfStudent: "+ listOfStudent.Count(),"Notice");
            string[,] tableOfResult = new string[listOfStudent.Count() * 2, numberOfProblemToBeGraded * 4 + 1];
            for (int i = 0; i <= listOfStudent.Count() - 1;i++ )
            {
                tableOfResult[i * 2, 0] = listOfStudent[i].studentID;
                tableOfResult[i * 2 + 1, 0] = "";
                for (int j = 0; j <= numberOfProblemToBeGraded - 1; j++)
                {
                    tableOfResult[i * 2, j * 4 + 1] = listOfStudent[i].listOfStaticsProblem[j].FBD_ATTEMPTED;
                    tableOfResult[i * 2 + 1, j * 4 + 1] = listOfStudent[i].listOfStaticsProblem[j].FBD_COMPLETED;
                    tableOfResult[i * 2, j * 4 + 2] = listOfStudent[i].listOfStaticsProblem[j].X_EQN_ATTEMPTED;
                    tableOfResult[i * 2 + 1, j * 4 + 2] = listOfStudent[i].listOfStaticsProblem[j].X_EQN_COMPLETED;
                    tableOfResult[i * 2, j * 4 + 3] = listOfStudent[i].listOfStaticsProblem[j].Y_EQN_ATTEMPTED;
                    tableOfResult[i * 2 + 1, j * 4 + 3] = listOfStudent[i].listOfStaticsProblem[j].Y_EQN_COMPLETED;
                    tableOfResult[i * 2, j * 4 + 4] = listOfStudent[i].listOfStaticsProblem[j].M_EQN_ATTEMPTED;
                    tableOfResult[i * 2 + 1, j * 4 + 4] = listOfStudent[i].listOfStaticsProblem[j].M_EQN_COMPLETED;
                }
            }
            using(System.IO.StreamWriter sw = new System.IO.StreamWriter(textBox3.Text+"\\GRADE.csv"))
            {
                for (int i = 0; i <= tableOfResult.GetLength(0) - 1;i++ )
                {
                    for (int j = 0; j <= tableOfResult.GetLength(1) - 1;j++ )
                    {
                        sw.Write(tableOfResult[i,j] + ",");
                    }
                    sw.Write("\n");
                }
            }
            MessageBox.Show("The grades had been saved.", "Notice");
        }

        private string[] concatenateAllTextFileIntoOneArray(string[] arrayOfFileLocations)
        {
            List<string> output = new List<string>();
            foreach(string dir in arrayOfFileLocations)
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(dir))
                {
                    while (!sr.EndOfStream)
                    {
                        output.Add(sr.ReadLine());
                    }
                }
            }
            return output.ToArray();
        }

        private Student[] autoGradeTheSummaryFiles(string[] dataToBeGraded, int numberOfProblemToGrade)
        {
            List<Student> listOfStudentWithGrades = new List<Student>();
            int currentProblemIndex = -1;
            // MessageBox.Show("Size of dataToBeGraded: " + dataToBeGraded.Count(), "Notice");
            for (int i = 0; i <= dataToBeGraded.Count() - 1;i++ )
            {
                string lineToBeChecked = dataToBeGraded[i];
                if (lineToBeChecked.StartsWith("Student ID:"))
                {
                    currentProblemIndex = -1;
                    string[] s = lineToBeChecked.Split(' ');
                    string localStudentID = s[s.Count()-1];
                    listOfStudentWithGrades.Add(new Student(localStudentID, numberOfProblemToGrade));
                }

                if (lineToBeChecked.StartsWith("The user is solving:"))
                {
                    string[] s = lineToBeChecked.Split(' ');
                    for (int j = 0; j <= numberOfProblemToGrade - 1;j++ )
                    {
                        if (s[s.Count()-1].Equals(nameOfStaticsProblem[j]))
                            currentProblemIndex = j;                        
                    }
                }

                if (lineToBeChecked.StartsWith("FBD ATTEMPTED"))
                {
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].FBD_ATTEMPTED = "1";
                }

                if (lineToBeChecked.StartsWith("FBD COMPLETED"))
                {
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].FBD_ATTEMPTED = "1";
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].FBD_COMPLETED = "1";
                }

                if (lineToBeChecked.StartsWith("X EQUATION ATTEMPTED"))
                {
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].X_EQN_ATTEMPTED = "1";
                }

                if (lineToBeChecked.StartsWith("Y EQUATION ATTEMPTED"))
                {
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].Y_EQN_ATTEMPTED = "1";
                }

                if (lineToBeChecked.StartsWith("MOMENT EQUATION ATTEMPTED"))
                {
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].M_EQN_ATTEMPTED = "1";
                }

                if (lineToBeChecked.StartsWith("Equation: FX=0"))
                {
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].X_EQN_ATTEMPTED = "1";
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].X_EQN_COMPLETED = "1";
                }

                if (lineToBeChecked.StartsWith("Equation: FY=0"))
                {
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].Y_EQN_ATTEMPTED = "1";
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].Y_EQN_COMPLETED = "1";
                }

                if (lineToBeChecked.StartsWith("Equation: M"))
                {
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].M_EQN_ATTEMPTED = "1";
                    listOfStudentWithGrades[listOfStudentWithGrades.Count() - 1].listOfStaticsProblem[currentProblemIndex].M_EQN_COMPLETED = "1";
                }
            }

            return listOfStudentWithGrades.ToArray();
        }
    }
}
