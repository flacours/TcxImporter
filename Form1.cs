using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;

namespace TcxImporter
{
    public partial class Form1 : Form
    {
        private string SelectedFile { get; set; }
        private Processor m_proc;
        private DataTable m_dt;

        public Form1()
        {
            InitializeComponent();
            m_proc = new Processor(this);
            textBoxOutputMax.Text = m_proc.MaxPoints.ToString();
        }


        private void buttonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Tcx files|*.tcx|All files |*.*";
            dialog.ShowDialog();
            SelectedFile = dialog.FileName;
            PrintDiag(SelectedFile, DiagType.Out);
            if (string.IsNullOrEmpty(SelectedFile)) return;
            int maxPoints;
            if (Int32.TryParse(textBoxOutputMax.Text, out maxPoints))
            {
                m_proc.MaxPoints = maxPoints;
            }
            m_dt = m_proc.DoConvert(SelectedFile);
            dataGridView1.DataSource = m_dt;
            PrintDiag("Number of locations : " + m_dt.Rows.Count, DiagType.Success);
        }


        internal void PrintDiag(string message, DiagType type)
        {
            textBox1.Text = message;
            switch (type)
            {
                case DiagType.Error:
                    textBox1.BackColor = Color.Red;
                    break;
                case DiagType.Out:
                    textBox1.BackColor = this.textBox1.BackColor = System.Drawing.SystemColors.Control;
                    break;
                case DiagType.Success:
                    textBox1.BackColor = Color.YellowGreen;
                    break;
            }
        }
    

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(SelectedFile)) {
                PrintDiag("Select a file first.", DiagType.Error);
                return;
            }
            string fileName = Path.GetFileNameWithoutExtension(SelectedFile) + ".csv";
            m_proc.SaveFile(fileName, m_dt);
        }
    }
}
