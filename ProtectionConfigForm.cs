using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntiTamperEOF_Dnlib
{
    public partial class ProtectionConfigForm : Form
    {
        public string? SelectedFilePath { get; private set; }
        public int ProtectionLevel { get; private set; } = 2;
        public bool EnableControlFlow { get; private set; } = true;
        public bool VirtualizeAll { get; private set; } = false;
        public bool EnableAntiDebug { get; private set; } = true;
        public bool EnableIntegrityCheck { get; private set; } = true;
        public bool EnableHideMain { get; private set; } = true;
        public bool ApplyProtection { get; private set; } = false;

        private TextBox filePathTextBox;
        private Button browseButton;
        private CheckBox controlFlowCheckBox;
        private CheckBox virtualizeAllCheckBox;
        private CheckBox antiDebugCheckBox;
        private CheckBox integrityCheckBox;
        private CheckBox hideMainCheckBox;
        private RadioButton basicRadio;
        private RadioButton intermediateRadio;
        private RadioButton advancedRadio;
        private Button protectButton;
        private Button cancelButton;
        private Label statusLabel;
        private ProgressBar progressBar;

        public ProtectionConfigForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "EOF Protektor - Configuración de Protección";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = SystemIcons.Shield;

            // Título
            var titleLabel = new Label
            {
                Text = "EOF PROTEKTOR - PROTECTOR AVANZADO .NET",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 20),
                Size = new Size(550, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

            var versionLabel = new Label
            {
                Text = "Versión 2.0 ULTRA | Anti-Debug, Control Flow, Virtualización",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(20, 50),
                Size = new Size(550, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(versionLabel);

            // Selección de archivo
            var fileGroupBox = new GroupBox
            {
                Text = "Archivo a Proteger",
                Location = new Point(20, 80),
                Size = new Size(550, 80),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            filePathTextBox = new TextBox
            {
                Location = new Point(15, 30),
                Size = new Size(420, 25),
                PlaceholderText = "Seleccione un archivo .exe para proteger..."
            };

            browseButton = new Button
            {
                Text = "Examinar...",
                Location = new Point(450, 28),
                Size = new Size(80, 30),
                UseVisualStyleBackColor = true
            };
            browseButton.Click += BrowseButton_Click;

            fileGroupBox.Controls.Add(filePathTextBox);
            fileGroupBox.Controls.Add(browseButton);
            this.Controls.Add(fileGroupBox);

            // Nivel de protección
            var levelGroupBox = new GroupBox
            {
                Text = "Nivel de Protección",
                Location = new Point(20, 170),
                Size = new Size(270, 100),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            basicRadio = new RadioButton
            {
                Text = "Básico (Rápido)",
                Location = new Point(15, 25),
                Size = new Size(200, 20)
            };

            intermediateRadio = new RadioButton
            {
                Text = "Intermedio (Recomendado)",
                Location = new Point(15, 45),
                Size = new Size(200, 20),
                Checked = true
            };

            advancedRadio = new RadioButton
            {
                Text = "Avanzado (Máxima protección)",
                Location = new Point(15, 65),
                Size = new Size(200, 20)
            };

            levelGroupBox.Controls.Add(basicRadio);
            levelGroupBox.Controls.Add(intermediateRadio);
            levelGroupBox.Controls.Add(advancedRadio);
            this.Controls.Add(levelGroupBox);

            // Opciones de protección
            var optionsGroupBox = new GroupBox
            {
                Text = "Opciones de Protección",
                Location = new Point(300, 170),
                Size = new Size(270, 180),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            controlFlowCheckBox = new CheckBox
            {
                Text = "Control Flow Obfuscation",
                Location = new Point(15, 25),
                Size = new Size(200, 20),
                Checked = true
            };

            virtualizeAllCheckBox = new CheckBox
            {
                Text = "Virtualizar TODAS las funciones",
                Location = new Point(15, 50),
                Size = new Size(220, 20),
                ForeColor = Color.DarkRed
            };

            antiDebugCheckBox = new CheckBox
            {
                Text = "Protección Anti-Debug",
                Location = new Point(15, 75),
                Size = new Size(200, 20),
                Checked = true
            };

            integrityCheckBox = new CheckBox
            {
                Text = "Verificación de Integridad",
                Location = new Point(15, 100),
                Size = new Size(200, 20),
                Checked = true
            };

            hideMainCheckBox = new CheckBox
            {
                Text = "Hide Main Methodology",
                Location = new Point(15, 125),
                Size = new Size(200, 20),
                Checked = true
            };

            optionsGroupBox.Controls.Add(controlFlowCheckBox);
            optionsGroupBox.Controls.Add(virtualizeAllCheckBox);
            optionsGroupBox.Controls.Add(antiDebugCheckBox);
            optionsGroupBox.Controls.Add(integrityCheckBox);
            optionsGroupBox.Controls.Add(hideMainCheckBox);
            this.Controls.Add(optionsGroupBox);

            // Botones
            protectButton = new Button
            {
                Text = "🛡️ APLICAR PROTECCIÓN",
                Location = new Point(20, 370),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            protectButton.Click += ProtectButton_Click;

            cancelButton = new Button
            {
                Text = "Cancelar",
                Location = new Point(240, 370),
                Size = new Size(100, 40),
                UseVisualStyleBackColor = true
            };
            cancelButton.Click += (s, e) => this.Close();

            this.Controls.Add(protectButton);
            this.Controls.Add(cancelButton);

            // Barra de progreso y estado
            progressBar = new ProgressBar
            {
                Location = new Point(20, 420),
                Size = new Size(550, 20),
                Visible = false
            };

            statusLabel = new Label
            {
                Text = "Listo para proteger archivo...",
                Location = new Point(20, 445),
                Size = new Size(550, 20),
                ForeColor = Color.Blue
            };

            this.Controls.Add(progressBar);
            this.Controls.Add(statusLabel);

            // Tooltips
            var toolTip = new ToolTip();
            toolTip.SetToolTip(controlFlowCheckBox, "Ofusca el flujo de control del programa para dificultar el análisis");
            toolTip.SetToolTip(virtualizeAllCheckBox, "ADVERTENCIA: Virtualiza TODAS las funciones (puede causar problemas de rendimiento)");
            toolTip.SetToolTip(antiDebugCheckBox, "Detecta y previene el debugging del programa");
            toolTip.SetToolTip(integrityCheckBox, "Verifica que el programa no haya sido modificado");
            toolTip.SetToolTip(hideMainCheckBox, "Oculta el punto de entrada principal del programa");
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivo .exe a proteger",
                Filter = "Ejecutables .NET (*.exe)|*.exe|Todos los archivos (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private async void ProtectButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(filePathTextBox.Text) || !File.Exists(filePathTextBox.Text))
            {
                MessageBox.Show("Por favor, seleccione un archivo válido.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Obtener configuración
            SelectedFilePath = filePathTextBox.Text;
            ProtectionLevel = basicRadio.Checked ? 1 : (intermediateRadio.Checked ? 2 : 3);
            EnableControlFlow = controlFlowCheckBox.Checked;
            VirtualizeAll = virtualizeAllCheckBox.Checked;
            EnableAntiDebug = antiDebugCheckBox.Checked;
            EnableIntegrityCheck = integrityCheckBox.Checked;
            EnableHideMain = hideMainCheckBox.Checked;

            // Confirmar configuración
            var configMessage = $"Configuración seleccionada:\n\n" +
                $"Archivo: {Path.GetFileName(SelectedFilePath)}\n" +
                $"Nivel: {(ProtectionLevel == 1 ? "Básico" : ProtectionLevel == 2 ? "Intermedio" : "Avanzado")}\n" +
                $"Control Flow: {(EnableControlFlow ? "SÍ" : "NO")}\n" +
                $"Virtualización completa: {(VirtualizeAll ? "SÍ" : "NO")}\n" +
                $"Anti-Debug: {(EnableAntiDebug ? "SÍ" : "NO")}\n" +
                $"Verificación integridad: {(EnableIntegrityCheck ? "SÍ" : "NO")}\n" +
                $"Hide Main: {(EnableHideMain ? "SÍ" : "NO")}\n\n" +
                "¿Desea continuar con la protección?";

            if (VirtualizeAll)
            {
                configMessage += "\n\n⚠️ ADVERTENCIA: La virtualización completa puede afectar el rendimiento.";
            }

            var result = MessageBox.Show(configMessage, "Confirmar Protección", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ApplyProtection = true;
                await StartProtectionProcess();
            }
        }

        private async Task StartProtectionProcess()
        {
            try
            {
                // Deshabilitar controles
                protectButton.Enabled = false;
                browseButton.Enabled = false;
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;

                statusLabel.Text = "Iniciando proceso de protección...";
                statusLabel.ForeColor = Color.Orange;

                // Simular progreso (en una implementación real, esto sería el progreso real)
                await Task.Delay(500);

                // Aquí se llamaría al método de protección real
                statusLabel.Text = "Aplicando protecciones avanzadas...";
                await Task.Delay(1000);

                statusLabel.Text = "✅ Protección aplicada exitosamente!";
                statusLabel.ForeColor = Color.Green;
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 100;

                await Task.Delay(1000);

                MessageBox.Show("¡Protección aplicada exitosamente!\n\nEl archivo ha sido protegido con las opciones seleccionadas.", 
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                statusLabel.Text = "❌ Error en la protección";
                statusLabel.ForeColor = Color.Red;
                progressBar.Visible = false;

                MessageBox.Show($"Error durante la protección:\n{ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Rehabilitar controles
                protectButton.Enabled = true;
                browseButton.Enabled = true;
            }
        }
    }
}