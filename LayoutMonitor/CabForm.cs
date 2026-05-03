using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LayoutMonitor
{
    public partial class CabForm : Form
    {
        public string DCCID
        {
            get { return lblDCCID.Text; }
            set { lblDCCID.Text = value; }
        }

        public string TrainName
        {
            set { lblTrainName.Text = value; ; }
        }

        public string NextBlock
        {
            set { lblNextBlock.Text = value; }
        }

        public string NextNextBlock
        {
            set { lblTwoBlock.Text = value; }
        }

        public string SignalAspect
        {
            set
            {
                pbSignalBox.Load("./Assets/" + value + ".png");
            }
        }

        public string CurrentBlock
        {
            set { lblCurrentBlock.Text = value; }
        }
        public CabForm()
        {
            InitializeComponent();
            lblDCCID.Text = DCCID;
        }
    }
}
