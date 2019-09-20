using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UFH.Application.Views
{
    public partial class PopupWindow : Form
    {
        public PopupWindow()
        {
            InitializeComponent();
        }

        public string GetCode()
        {
            string code = textBox1.Text + textBox2.Text + textBox3.Text + textBox4.Text + textBox5.Text + textBox6.Text;
            return code;
        }

        private void PopupWindow_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }

            if (e.KeyChar == 8)
            {
                ((TextBox)sender).Clear();
                textBox1.Focus();
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            if (e.KeyChar == 8)
            {
                ((TextBox)sender).Clear();
                textBox1.Focus();
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            if (e.KeyChar == 8)
            {
                ((TextBox)sender).Clear();
                textBox2.Focus();
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            if (e.KeyChar == 8)
            {
                ((TextBox)sender).Clear();
                textBox3.Focus();
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            if (e.KeyChar == 8)
            {
                ((TextBox)sender).Clear();
                textBox4.Focus();
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            if (e.KeyChar == 8)
            {
                ((TextBox)sender).Clear();
                textBox5.Focus();
            }
        }


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != " " && textBox2.Text != " " && textBox3.Text != " " &&
                textBox4.Text != " " && textBox5.Text != " " && textBox6.Text != " ")
            {
                Close();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Control nextInTabOrder = GetNextControl(this, true);
            nextInTabOrder.Focus();
            textBox2.Focus();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Control nextInTabOrder = GetNextControl(this, true);
            nextInTabOrder.Focus();
            textBox3.Focus();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Control nextInTabOrder = GetNextControl(this, true);
            nextInTabOrder.Focus();
            textBox4.Focus();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Control nextInTabOrder = GetNextControl(this, true);
            nextInTabOrder.Focus();
            textBox5.Focus();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            Control nextInTabOrder = GetNextControl(this, true);
            nextInTabOrder.Focus();
            textBox6.Focus();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            Control nextInTabOrder = GetNextControl(this, true);
            nextInTabOrder.Focus();
            buttonSubmit.Focus();
        }
    }
}
