using System.Windows.Forms;

namespace WebBrowser
{
    public partial class Password : Form
    {
        public Password()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                this.Hide();
            }
        }
    }
}
