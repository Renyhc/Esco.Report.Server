using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace esco.report.server
{
    public partial class Progress : Form
    {
        public Progress()
        {
            InitializeComponent();
        }

        public void SetProgress(string value)
        {
            this.progressBar.Value = Int32.Parse(value);
            this.progressPercent.Text = value + "%";
        }
    }
}
