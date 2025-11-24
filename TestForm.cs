using System;
using System.Drawing;
using System.Windows.Forms;

namespace AntiTamperEOF_Dnlib
{
    public class TestForm : Form
    {
        public TestForm()
        {
            this.Text = "Test Form";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            var label = new Label
            {
                Text = "¡Hola! La interfaz gráfica funciona correctamente.",
                Location = new Point(50, 50),
                Size = new Size(300, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            var button = new Button
            {
                Text = "Cerrar",
                Location = new Point(150, 150),
                Size = new Size(100, 30)
            };
            button.Click += (s, e) => this.Close();
            
            this.Controls.Add(label);
            this.Controls.Add(button);
        }
    }
}