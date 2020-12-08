using System;
using System.Collections.Generic; // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PA7_Draft
{
    public partial class MainForm : Form
    {
        private Worker SortMachine;
        private BackgroundWorker[] Threads;
        private ProgressBar[] Bars;
        private TextBox[] Labels;
        private TextBox[] Files;
        public MainForm()
        {
            InitializeComponent();
            SortMachine = new Worker();
            listBox1.DataSource = SortMachine.WaitingQueue;
            Threads = new BackgroundWorker[8];
            Bars = new ProgressBar[8];
            Labels = new TextBox[8];
            Files = new TextBox[8];
            for (int i = 0; i < 8; i++)
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


        private void ListBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                /*
                bool wrongExtension = false;
                foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
                    if (System.IO.Path.GetExtension(file).ToUpperInvariant() != ".TXT")
                        wrongExtension = true;
                Console.WriteLine(wrongExtension);
                if (wrongExtension)
                    e.Effect = DragDropEffects.None;
                
                else
                */
                    e.Effect = DragDropEffects.Copy;
            }                
        }

        private void ListBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                this.SortMachine.WaitingQueue.Add(file); 
            SortMachine.IncompleteTasks += files.Length;// ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
            AssignTask();  // ANALIZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
        }

        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
        private void AssignTask()
        {
            List<string> removed = new List<string>();
            foreach (string file in SortMachine.WaitingQueue)
            {
                int i;
                for (i = 0; i < 8; i++)                
                    if (!Threads[i].IsBusy)
                    {
                        Threads[i].RunWorkerAsync(file);
                        break;
                    }
                if (i == 8)
                    break;
                removed.Add(file);                
            }
            foreach (string r in removed)            
                SortMachine.WaitingQueue.Remove(r);
            UpdateStatusBar();
        }
        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE

        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
        private void BackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string filemame = (string)e.Argument;

            if(SortMachine.WorkingSet.ContainsKey(filemame))
            {
                e.Result = "error";
                return;
            }

            SortingTask task = new SortingTask(filemame, (BackgroundWorker)sender);
            while (!SortMachine.WorkingSet.TryAdd(filemame, task));
            SortMachine.LoadSortAndSave(filemame);
            e.Result = "success";
        }
        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE

        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
        private void BackGroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                if (sender.Equals(Threads[i]))
                {
                    Bars[i].Value = e.ProgressPercentage;
                    if(((string)e.UserState).StartsWith("Loading"))
                    {
                        Files[i].Text = ((string)e.UserState).Substring(8, ((string)e.UserState).Length - 8);
                        Bars[i].Visible = true;
                        Files[i].Visible = true;
                        Labels[i].Visible = true;
                        Labels[i].Text = "Loading file, please wait...";
                    }
                    else if (((string)e.UserState).StartsWith("Saving"))
                    {
                        Labels[i].Text = "Saving sorted file, please wait...";
                    }
                    else
                    {
                        Labels[i].Text = "Sorting, please wait...";
                    }
                    break;
                }
            }
            UpdateStatusBar();
        }
        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE

        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
        private void BackGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (((string)e.Result).Equals("error"))
                return;

            for (int i = 0; i < 8; i++)            
                if (sender.Equals(Threads[i]))
                {
                    Bars[i].Visible = false;
                    Files[i].Visible = false;
                    Labels[i].Visible = false;
                    SortingTask task = SortMachine.WorkingSet[Files[i].Text];
                    while (!SortMachine.WorkingSet.TryRemove(Files[i].Text, out task));
                    break;
                }           
            AssignTask();
        }
        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE

        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
        private void UpdateStatusBar()
        {
            if(SortMachine.WaitingQueue.Count + SortMachine.WorkingSet.Count == 0)
            {
                SortMachine.IncompleteTasks = 0;
                toolStripProgressBar1.Value = 0;
                toolStripStatusLabel1.Text = "Ready";
                return;
            }

            toolStripProgressBar1.Value = (int)Math.Floor(0.10 * (SortMachine.IncompleteTasks - Math.Min(SortMachine.IncompleteTasks, SortMachine.WaitingQueue.Count)));
            //toolStripProgressBar1.Value = (int)Math.Floor(100.0 * (SortMachine.IncompleteTasks - Math.Min(SortMachine.IncompleteTasks, SortMachine.WaitingQueue.Count)));
            toolStripStatusLabel1.Text = SortMachine.WaitingQueue.Count + " files are waiting... " + SortMachine.WorkingSet.Count + " files are being sorted";

        }
        // ANALYZEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
    }
}

//return (int)Math.Floor(100 * Math.Min(1, Progress / EstimatedComparisons));