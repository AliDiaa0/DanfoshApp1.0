using System;
using System.Media;
using System.Windows.Forms;

namespace DanfoshApp
{
    public partial class DanfoshForm : Form
    {
        public DanfoshForm()
        {
            InitializeComponent();
        }

        private void DanfoshForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void DanfoshForm_Load(object sender, EventArgs e)
        {
            // Play the music
            SoundPlayer snd = new SoundPlayer(Properties.Resources.nokia_bulbul);
            snd.PlayLooping();
        }
    }
}