// Imports.
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace PA7_Draft
{
    // Main Class.
    public partial class MainForm : Form
    {
        // Global Variables.
        private Worker SortMachine;
        private BackgroundWorker[] Threads;
        private ProgressBar[] Bars;
        private TextBox[] Labels;
        private TextBox[] Files;

        // Global Variables For Messages. 
        private string errorMessage = "bug";
        private string readyMessage = "Ready!";
        private string loadingMessage = "Loading";
        private string savingMessage = "Comitting";
        private string successMessage = "Realization";
        private string fileWaitMessage = " files are waiting... ";
        private string fileSortMessage = " files are being sorted!";
        private string sortingWaitMessage = "Sorting, please wait...";
        private string loadingWaitMessage = "Loading file, please wait...";
        private string savingWaitMessage = "Comitting sorted file, please wait...";

        // Global Variable Custom.
        // Only doing 8 proceses at a time.
        private int ProcessSameTime = 8;

        // Constructor.
        public MainForm()
        {
            InitializeComponent();
            SortMachine = new Worker();
            listBox1.DataSource = SortMachine.WaitingQueue;
            Threads = new BackgroundWorker[ProcessSameTime];
            Bars = new ProgressBar[ProcessSameTime];
            Labels = new TextBox[ProcessSameTime];
            Files = new TextBox[ProcessSameTime];
            for (int i = 0; i < ProcessSameTime; i++)
            {
                Threads[i]= new BackgroundWorker();
                Bars[i] = new ProgressBar();
                Labels[i] = new TextBox();
                Files[i] = new TextBox();
                tableLayoutPanel1.Controls.Add(Labels[i], 2, i);
                tableLayoutPanel1.Controls.Add(Files[i], 0, i);
                tableLayoutPanel1.Controls.Add(Bars[i], 1, i);
                Bars[i].Dock = DockStyle.Fill;
                Labels[i].Dock = DockStyle.Fill;
                Files[i].Dock = DockStyle.Fill;
                Labels[i].BackColor = SystemColors.Menu;
                Files[i].BackColor = SystemColors.Menu;
                Labels[i].Multiline = true;
                Files[i].Multiline = true;
                Labels[i].Enabled = false;
                Files[i].Enabled = false;
                Labels[i].ScrollBars = ScrollBars.Vertical;
                Files[i].ScrollBars = ScrollBars.Vertical;
                Bars[i].Visible = false;
                Labels[i].Visible = false;
                Files[i].Visible = false;
                Threads[i].WorkerReportsProgress = true;
                Threads[i].DoWork += new DoWorkEventHandler(BackGroundWorker_DoWork);
                Threads[i].ProgressChanged += new ProgressChangedEventHandler(BackGroundWorker_ProgressChanged);
                Threads[i].RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackGroundWorker_RunWorkerCompleted);
            }
        }

        // Allows the effect of enter the files by dragging.
        // Validates for .txt files.
        private void ListBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bool wrongExtension = false;
                foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
                    if (System.IO.Path.GetExtension(file).ToUpperInvariant() != ".TXT")
                        wrongExtension = true;
                Console.WriteLine(wrongExtension);
                if (wrongExtension)
                    e.Effect = DragDropEffects.None;
                
                else
                    e.Effect = DragDropEffects.Copy;
            }                
        }

        private void ListBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                this.SortMachine.WaitingQueue.Add(file);

            // Setting the length of the files to the SortMachine.UnfinishedProcess variable created in the Worker class.
            SortMachine.UnfinishedProcess += files.Length;  

            // Set process from the list and remove them.
            // Calls the method to update the progress bar.
            SetRequest();  
        }
        
        // Method to execute the process.
        private void BackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Declarations.
            string txtDataFile = e.Argument.ToString();

            // If the list contains the txtDataFile show an error.
            if (SortMachine.WorkingSet.ContainsKey(txtDataFile))
            {
                // Update the Message.
                e.Result = errorMessage;

                // Return From the function.
                return;
            }

            // Object declaration.
            SortingTask assignedProcess = new SortingTask(txtDataFile, (BackgroundWorker)sender);

            // While Adding Wait.
            while (!SortMachine.WorkingSet.TryAdd(txtDataFile, assignedProcess)) ;  // While Adding Wait.
            SortMachine.LoadSortAndSave(txtDataFile);  /* Load and Sort Calling */
            e.Result = successMessage;  /* Update the message */
        }

        // Method to update the state of the proceses.
        private void BackGroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Only doing 8 proceses at a time.
            for (int i = 0; i < ProcessSameTime; i++)
            {
                // Checking if both objects are the same.
                if (sender.Equals(Threads[i]))
                {
                    // Assigninig ProgressChanged to the multiple progress bars.
                    Bars[i].Value = e.ProgressPercentage;

                    // If the current state is loading.
                    if ((e.UserState.ToString()).StartsWith(loadingMessage))  
                    {
                        // Retrieves the string.
                        Files[i].Text = ((string)e.UserState).Substring(ProcessSameTime, ((string)e.UserState).Length - ProcessSameTime);

                        // Setting the message.
                        Labels[i].Text = loadingWaitMessage;

                        // Assigning some properties.
                        Bars[i].Visible = true;
                        Files[i].Visible = true;
                        Labels[i].Visible = true;
                    }
                    // If the current state is saving.
                    else if (((string)e.UserState).StartsWith(savingMessage))  // To String Maybe 
                    {
                        // Updating the message of the bars.
                        Labels[i].Text = savingWaitMessage;
                    }
                    // Else Sorting.
                    else
                    {
                        // Updating the message of the bars.
                        Labels[i].Text = sortingWaitMessage;
                    }

                    // If None of the above stop the loop.
                    i = ProcessSameTime;
                }
            }

            // Updates the value and messages of the ToolStripProgressBarOne.
            // Sets propeties of the ToolStripProgressBarOne.
            UpgradeToolStripProgressBarOne();
        }

        // Method to complete the process.
        private void BackGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // If there is an error then is not completed.
            if (e.Result.ToString().Equals(errorMessage)) 
            {
                // Exit Function.
                return;
            }

            // If there is not error.
            // Only doing 8 proceses at a time.
            for (int i = 0; i < ProcessSameTime; i++)
            {
                // Checking if both objects are the same.
                if (sender.Equals(Threads[i]))
                {
                    // Assigning some properties.
                    Bars[i].Visible = false;
                    Files[i].Visible = false;
                    Labels[i].Visible = false;

                    // Creating the object
                    SortingTask processObject = SortMachine.WorkingSet[Files[i].Text];

                    // Remove a Process Not Stop the For Loop.
                    while (!SortMachine.WorkingSet.TryRemove(Files[i].Text, out processObject)) ;
                    {
                        // Stop the loop.
                        i = ProcessSameTime;  
                    }
                }
            }

            // Set process from the list and remove them.
            // Calls the method to update the progress bar.
            SetRequest();
        }

        // Updates the value and messages of the ToolStripProgressBarOne.
        // Sets propeties of the ToolStripProgressBarOne.
        private void UpgradeToolStripProgressBarOne()
        {
            // If there are not more process update.
            if (SortMachine.WaitingQueue.Count + SortMachine.WorkingSet.Count == 0)
            {
                // Update the progress bar value.
                toolStripProgressBar1.Value = 0;

                // Update the value for the new variable created in the class.  
                SortMachine.UnfinishedProcess = 0;

                // Update the the message of the tool strip.
                toolStripStatusLabel1.Text = readyMessage;

                // Return to avoid updating wron values in lines below.
                return; 
            }
            else
            {
                // If there are more process Update the value of the progress bat accordinly.
                toolStripProgressBar1.Value = (int)Math.Floor(7.0 * (SortMachine.UnfinishedProcess - Math.Min(SortMachine.UnfinishedProcess, SortMachine.WaitingQueue.Count)));

                // Update the the message of the tool strip.
                toolStripStatusLabel1.Text = SortMachine.WaitingQueue.Count + fileWaitMessage + SortMachine.WorkingSet.Count + fileSortMessage; 
            }
        }

        // Set process from the list and remove them.
        // Calls the method to update the progress bar.
        private void SetRequest()
        {
            // Declarations.
            List<string> takeAwayList = new List<string>();

            // Processing the files.
            foreach (string txtDataFile in SortMachine.WaitingQueue)
            {
                // Temporary Declaration inside the foreach loop.
                int varLoop;

                // Only doing 8 proceses at a time.
                for (varLoop = 0; varLoop < ProcessSameTime; varLoop++)
                {
                    // If not busy.
                    if (!Threads[varLoop].IsBusy)
                    {
                        // Sync the Proceses.
                        Threads[varLoop].RunWorkerAsync(txtDataFile);

                        // Stop the foreach loop.
                        break;
                    }
                }

                //Only doing 8 proceses at a time.
                if (varLoop == ProcessSameTime)
                {
                    // Stop the foreach loop.
                    break;
                }

                // If less than 8 add a new file to the process.
                takeAwayList.Add(txtDataFile);
            }

            // Removed from the list the finished proceses.
            foreach (string removedTxtDataFile in takeAwayList)
            {
                // Remove the element from the list.
                SortMachine.WaitingQueue.Remove(removedTxtDataFile);
            }

            // Update the Main Progress Bar.
            UpgradeToolStripProgressBarOne();
        }        
    }
}