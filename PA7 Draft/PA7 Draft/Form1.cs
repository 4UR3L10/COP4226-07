using System;
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
        }


        private void BackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
  
        }

        private void BackGroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
 
        }

        private void BackGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }
    }
}
