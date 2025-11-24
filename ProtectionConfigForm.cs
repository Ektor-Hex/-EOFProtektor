using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using dnlib.DotNet;
using EOFProtektor.Core;
using EOFProtektor.Protection;
using EOFProtektor.Obfuscation;

namespace EOFProtektor
{
    public partial class ProtectionConfigForm : Form
    {
        private TextBox filePathTextBox = null!;
        private Button browseButton = null!;
        private CheckBox controlFlowCheckBox = null!;
        private CheckBox virtualizeAllCheckBox = null!;
        private CheckBox antiDebugCheckBox = null!;
        private CheckBox hideMainCheckBox = null!;
        private CheckBox integrityCheckBox = null!;
        private RadioButton basicRadio = null!;
        private RadioButton intermediateRadio = null!;
        private RadioButton advancedRadio = null!;
        private RadioButton customRadio = null!;
        private Button protectButton = null!;
        private RichTextBox logTextBox = null!;
        private ProgressBar progressBar = null!;
        private Label statusLabel = null!;
        private Panel configPanel = null!;
        private Panel logPanel = null!;
        private Label fileInfoLabel = null!;
        private Panel customOptionsPanel = null!;
        private bool isProcessing = false;

        public ProtectionConfigForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "EOF Protektor v2.0 - Advanced .NET Protector";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 245);

            // Panel de configuración (izquierda)
            configPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(450, 700),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(configPanel);

            // Header
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(450, 80),
                BackColor = Color.FromArgb(45, 45, 70)
            };
            configPanel.Controls.Add(headerPanel);

            var titleLabel = new Label
            {
                Text = "🛡️ EOF PROTEKTOR",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                Size = new Size(400, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(titleLabel);

            var subtitleLabel = new Label
            {
                Text = "Advanced .NET Protection & Obfuscation",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                Location = new Point(20, 48),
                Size = new Size(400, 20)
            };
            headerPanel.Controls.Add(subtitleLabel);

            // Archivo
            var fileLabel = new Label
            {
                Text = "Archivo a Proteger",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 100),
                Size = new Size(200, 25)
            };
            configPanel.Controls.Add(fileLabel);

            filePathTextBox = new TextBox
            {
                Location = new Point(20, 130),
                Size = new Size(320, 25),
                Font = new Font("Segoe UI", 9),
                PlaceholderText = "Seleccione un archivo .exe o .dll..."
            };
            filePathTextBox.TextChanged += FilePathTextBox_TextChanged;
            configPanel.Controls.Add(filePathTextBox);

            browseButton = new Button
            {
                Text = "📁 Buscar",
                Location = new Point(350, 128),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            browseButton.FlatAppearance.BorderSize = 0;
            browseButton.Click += BrowseButton_Click;
            configPanel.Controls.Add(browseButton);

            fileInfoLabel = new Label
            {
                Text = "",
                Location = new Point(20, 160),
                Size = new Size(410, 40),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };
            configPanel.Controls.Add(fileInfoLabel);

            // Nivel de Protección
            var levelLabel = new Label
            {
                Text = "Nivel de Protección",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 210),
                Size = new Size(200, 25)
            };
            configPanel.Controls.Add(levelLabel);

            basicRadio = new RadioButton
            {
                Text = "⚡ Básico - Rápido y ligero",
                Location = new Point(30, 240),
                Size = new Size(380, 25),
                Font = new Font("Segoe UI", 9)
            };
            configPanel.Controls.Add(basicRadio);

            intermediateRadio = new RadioButton
            {
                Text = "⭐ Intermedio - Recomendado (balance perfecto)",
                Location = new Point(30, 270),
                Size = new Size(380, 25),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Checked = true,
                ForeColor = Color.DarkGreen
            };
            configPanel.Controls.Add(intermediateRadio);

            advancedRadio = new RadioButton
            {
                Text = "🔥 Avanzado - Máxima protección (puede tardar)",
                Location = new Point(30, 300),
                Size = new Size(380, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DarkRed
            };
            configPanel.Controls.Add(advancedRadio);

            customRadio = new RadioButton
            {
                Text = "⚙️ Custom - Personaliza cada protección",
                Location = new Point(30, 330),
                Size = new Size(380, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DarkMagenta
            };
            customRadio.CheckedChanged += CustomRadio_CheckedChanged;
            configPanel.Controls.Add(customRadio);

            // Panel de opciones personalizadas (oculto por defecto)
            customOptionsPanel = new Panel
            {
                Location = new Point(20, 365),
                Size = new Size(410, 160),
                BackColor = Color.FromArgb(245, 245, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            configPanel.Controls.Add(customOptionsPanel);

            var customLabel = new Label
            {
                Text = "Configuración Personalizada",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 5),
                Size = new Size(300, 20),
                ForeColor = Color.DarkMagenta
            };
            customOptionsPanel.Controls.Add(customLabel);

            antiDebugCheckBox = new CheckBox
            {
                Text = "🛡️ Anti-Debug Protection",
                Location = new Point(15, 30),
                Size = new Size(380, 22),
                Font = new Font("Segoe UI", 9),
                Checked = true
            };
            customOptionsPanel.Controls.Add(antiDebugCheckBox);

            hideMainCheckBox = new CheckBox
            {
                Text = "🎭 Hide Main (300 métodos falsos)",
                Location = new Point(15, 55),
                Size = new Size(380, 22),
                Font = new Font("Segoe UI", 9),
                Checked = true
            };
            customOptionsPanel.Controls.Add(hideMainCheckBox);

            controlFlowCheckBox = new CheckBox
            {
                Text = "🌀 Control Flow Obfuscation",
                Location = new Point(15, 80),
                Size = new Size(380, 22),
                Font = new Font("Segoe UI", 9),
                Checked = true
            };
            customOptionsPanel.Controls.Add(controlFlowCheckBox);

            integrityCheckBox = new CheckBox
            {
                Text = "🔐 Integrity Protection (EOF)",
                Location = new Point(15, 105),
                Size = new Size(380, 22),
                Font = new Font("Segoe UI", 9),
                Checked = true
            };
            customOptionsPanel.Controls.Add(integrityCheckBox);

            virtualizeAllCheckBox = new CheckBox
            {
                Text = "⚠️ Virtualizar TODO (experimental)",
                Location = new Point(15, 130),
                Size = new Size(380, 22),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DarkOrange,
                Checked = false
            };
            customOptionsPanel.Controls.Add(virtualizeAllCheckBox);

            // Status
            statusLabel = new Label
            {
                Text = "Listo - Seleccione un archivo para comenzar",
                Location = new Point(20, 540),
                Size = new Size(410, 40),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopLeft
            };
            configPanel.Controls.Add(statusLabel);

            // Progreso
            progressBar = new ProgressBar
            {
                Location = new Point(20, 590),
                Size = new Size(410, 25),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };
            configPanel.Controls.Add(progressBar);

            // Botón proteger
            protectButton = new Button
            {
                Text = "🛡️ APLICAR PROTECCIÓN",
                Location = new Point(20, 625),
                Size = new Size(410, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 150, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            protectButton.FlatAppearance.BorderSize = 0;
            protectButton.Click += ProtectButton_Click;
            configPanel.Controls.Add(protectButton);

            // Panel de log (derecha)
            logPanel = new Panel
            {
                Location = new Point(450, 0),
                Size = new Size(450, 700),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            this.Controls.Add(logPanel);

            var logHeaderLabel = new Label
            {
                Text = "📋 LOG DE PROTECCIÓN",
                Font = new Font("Consolas", 10, FontStyle.Bold),
                ForeColor = Color.LightGreen,
                Location = new Point(10, 10),
                Size = new Size(430, 25),
                BackColor = Color.FromArgb(20, 20, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            logPanel.Controls.Add(logHeaderLabel);

            logTextBox = new RichTextBox
            {
                Location = new Point(10, 45),
                Size = new Size(430, 645),
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            logPanel.Controls.Add(logTextBox);

            // Mensaje inicial
            LogMessage("=== EOF PROTEKTOR v2.0 ===", Color.Cyan);
            LogMessage("Sistema listo para proteger archivos .NET", Color.LightGray);
            LogMessage("", Color.White);
        }

        private void CustomRadio_CheckedChanged(object? sender, EventArgs e)
        {
            customOptionsPanel.Visible = customRadio.Checked;
            
            if (customRadio.Checked)
            {
                statusLabel.Text = "Modo Custom - Configure cada protección individualmente";
                statusLabel.ForeColor = Color.DarkMagenta;
            }
        }

        private void FilePathTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (File.Exists(filePathTextBox.Text))
            {
                try
                {
                    var fileInfo = new FileInfo(filePathTextBox.Text);
                    fileInfoLabel.Text = $"Tamaño: {fileInfo.Length / 1024} KB\nÚltima modificación: {fileInfo.LastWriteTime:dd/MM/yyyy HH:mm}";
                    fileInfoLabel.ForeColor = Color.Green;
                    protectButton.Enabled = !isProcessing;
                }
                catch
                {
                    fileInfoLabel.Text = "Archivo inválido";
                    fileInfoLabel.ForeColor = Color.Red;
                    protectButton.Enabled = false;
                }
            }
            else
            {
                fileInfoLabel.Text = "";
                protectButton.Enabled = false;
            }
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivo .NET a proteger",
                Filter = "Archivos .NET (*.exe;*.dll)|*.exe;*.dll|Ejecutables (*.exe)|*.exe|Bibliotecas (*.dll)|*.dll|Todos los archivos (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePathTextBox.Text = openFileDialog.FileName;
                LogMessage($"Archivo seleccionado: {Path.GetFileName(openFileDialog.FileName)}", Color.Yellow);
            }
        }

        private async void ProtectButton_Click(object? sender, EventArgs e)
        {
            if (isProcessing) return;
            if (string.IsNullOrWhiteSpace(filePathTextBox.Text) || !File.Exists(filePathTextBox.Text))
            {
                MessageBox.Show("Por favor, seleccione un archivo válido.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isProcessing = true;
            protectButton.Enabled = false;
            browseButton.Enabled = false;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            int level = basicRadio.Checked ? 1 : (intermediateRadio.Checked ? 2 : (advancedRadio.Checked ? 3 : 4));
            
            // En modo Custom, usar los valores de los checkboxes
            // En niveles predefinidos, usar valores según el nivel
            bool controlFlow, virtualizeAll, antiDebug, hideMain, integrity;
            
            if (customRadio.Checked)
            {
                // Modo Custom: usar valores exactos de los checkboxes
                antiDebug = antiDebugCheckBox.Checked;
                hideMain = hideMainCheckBox.Checked;
                controlFlow = controlFlowCheckBox.Checked;
                integrity = integrityCheckBox.Checked;
                virtualizeAll = virtualizeAllCheckBox.Checked;
            }
            else
            {
                // Niveles predefinidos: configuración automática
                antiDebug = true; // Siempre en niveles 1+
                hideMain = (level >= 2); // Nivel 2+
                controlFlow = (level >= 2); // Nivel 2+
                integrity = true; // Siempre
                virtualizeAll = false; // Solo si usuario lo activa en Custom
            }

            LogMessage("", Color.White);
            LogMessage("═══════════════════════════════════", Color.Cyan);
            LogMessage("INICIANDO PROTECCIÓN", Color.LightGreen);
            LogMessage("═══════════════════════════════════", Color.Cyan);
            
            if (customRadio.Checked)
            {
                LogMessage("Modo: CUSTOM (Personalizado)", Color.Magenta);
                LogMessage($"  • Anti-Debug: {(antiDebug ? "SÍ" : "NO")}", Color.White);
                LogMessage($"  • Hide Main: {(hideMain ? "SÍ" : "NO")}", Color.White);
                LogMessage($"  • Control Flow: {(controlFlow ? "SÍ" : "NO")}", Color.White);
                LogMessage($"  • Integrity: {(integrity ? "SÍ" : "NO")}", Color.White);
                LogMessage($"  • Virtualización: {(virtualizeAll ? "COMPLETA" : "NO")}", Color.White);
            }
            else
            {
                LogMessage($"Nivel: {(level == 1 ? "Básico" : level == 2 ? "Intermedio" : "Avanzado")}", Color.White);
                LogMessage($"  • Anti-Debug: Activado", Color.White);
                if (level >= 2)
                {
                    LogMessage($"  • Hide Main: Activado", Color.White);
                    LogMessage($"  • Control Flow: Activado", Color.White);
                }
                if (level >= 3)
                {
                    LogMessage($"  • Virtualización: Selectiva", Color.White);
                }
                LogMessage($"  • Integrity: Activado", Color.White);
            }
            LogMessage("", Color.White);

            try
            {
                await Task.Run(() => ApplyProtection(filePathTextBox.Text, level, controlFlow, virtualizeAll, antiDebug, hideMain, integrity));

                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 100;

                LogMessage("", Color.White);
                LogMessage("═══════════════════════════════════", Color.LightGreen);
                LogMessage("✓ PROTECCIÓN APLICADA EXITOSAMENTE", Color.LightGreen);
                LogMessage("═══════════════════════════════════", Color.LightGreen);

                MessageBox.Show("¡Protección aplicada exitosamente!\n\nEl archivo protegido ha sido guardado.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage("", Color.White);
                LogMessage("✗ ERROR EN LA PROTECCIÓN", Color.Red);
                LogMessage(ex.Message, Color.Red);

                MessageBox.Show($"Error durante la protección:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isProcessing = false;
                protectButton.Enabled = true;
                browseButton.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void ApplyProtection(string filePath, int level, bool controlFlow, bool virtualizeAll, bool antiDebug, bool hideMain, bool integrity)
        {
            var data = new ProtectionData();
            LogMessage($"→ Semilla generada: {data.Seed}", Color.Cyan);

            LogMessage("→ Cargando módulo .NET...", Color.Yellow);
            var mod = ModuleDefMD.Load(filePath);
            LogMessage($"  ✓ Módulo: {mod.Name}", Color.LightGray);
            LogMessage($"  ✓ Tipos encontrados: {mod.Types.Count}", Color.LightGray);

            LogMessage("", Color.White);

            // NIVEL 4: CUSTOM - Configuración personalizada
            if (level == 4)
            {
                LogMessage("→ Aplicando configuración CUSTOM...", Color.Magenta);

                if (antiDebug)
                {
                    LogMessage("  • Inyectando protección Anti-Debug", Color.White);
                    AntiDebugProtection.InjectUltimateAntiDebugClass(mod, data);
                    LogMessage("    ✓ Anti-Debug inyectado", Color.LightGreen);
                }
                else
                {
                    LogMessage("  ⊘ Anti-Debug omitido", Color.Gray);
                }

                if (hideMain)
                {
                    LogMessage("  • Aplicando Hide Main Obfuscation", Color.White);
                    HideMainObfuscator.ApplyHideMainObfuscation(mod, data);
                    LogMessage("    ✓ Hide Main aplicado", Color.LightGreen);
                }
                else
                {
                    LogMessage("  ⊘ Hide Main omitido", Color.Gray);
                }

                if (controlFlow)
                {
                    LogMessage("  • Aplicando Control Flow Obfuscation", Color.Orange);
                    ControlFlowObfuscator.ApplyAdvancedControlFlowObfuscation(mod, data);
                    LogMessage("    ✓ Control Flow aplicado", Color.LightGreen);
                }
                else
                {
                    LogMessage("  ⊘ Control Flow omitido", Color.Gray);
                }

                if (virtualizeAll)
                {
                    LogMessage("  • Virtualizando TODAS las clases", Color.Orange);
                    ClassVirtualizationObfuscator.ApplyClassVirtualization(mod, 3, true);
                    LogMessage("    ✓ Virtualización completa aplicada", Color.LightGreen);
                }
                else
                {
                    LogMessage("  ⊘ Virtualización omitida", Color.Gray);
                }
            }
            // NIVELES 1-3: Configuración predefinida
            else
            {
                LogMessage($"→ Aplicando protección nivel {level}...", Color.Yellow);

                // Aplicar protecciones según los parámetros recibidos
                if (antiDebug)
                {
                    LogMessage("  • Inyectando protección Anti-Debug", Color.White);
                    AntiDebugProtection.InjectUltimateAntiDebugClass(mod, data);
                    LogMessage("    ✓ Anti-Debug inyectado", Color.LightGreen);
                }

                if (hideMain)
                {
                    LogMessage("  • Aplicando Hide Main Obfuscation", Color.White);
                    HideMainObfuscator.ApplyHideMainObfuscation(mod, data);
                    LogMessage("    ✓ Hide Main aplicado", Color.LightGreen);
                }

                if (controlFlow)
                {
                    LogMessage("  • Aplicando Control Flow Obfuscation", Color.Orange);
                    ControlFlowObfuscator.ApplyAdvancedControlFlowObfuscation(mod, data);
                    LogMessage("    ✓ Control Flow aplicado", Color.LightGreen);
                }

                // Virtualización en nivel 3
                if (level >= 3)
                {
                    if (virtualizeAll)
                    {
                        LogMessage("  • Virtualizando TODAS las clases (puede tardar)", Color.Orange);
                        ClassVirtualizationObfuscator.ApplyClassVirtualization(mod, level, true);
                        LogMessage("    ✓ Virtualización completa aplicada", Color.LightGreen);
                    }
                    else
                    {
                        LogMessage("  • Virtualizando métodos selectos", Color.White);
                        ClassVirtualizationObfuscator.ApplyClassVirtualization(mod, level, false);
                        LogMessage("    ✓ Virtualización selectiva aplicada", Color.LightGreen);
                    }
                }
            }

            string outputPath = Path.Combine(
                Path.GetDirectoryName(filePath) ?? "",
                Path.GetFileNameWithoutExtension(filePath) + "_protected" + Path.GetExtension(filePath)
            );

            LogMessage("", Color.White);
            LogMessage("→ Guardando assembly protegido...", Color.Yellow);
            mod.Write(outputPath);
            LogMessage($"  ✓ Guardado en: {Path.GetFileName(outputPath)}", Color.LightGreen);
            LogMessage($"  ✓ Ruta completa: {outputPath}", Color.Gray);

            // Aplicar protección EOF solo si está habilitada
            if (level == 4 && !integrity)
            {
                LogMessage("", Color.White);
                LogMessage("⚠ Protección EOF multicapa omitida (desactivada en Custom)", Color.Yellow);
            }
            else
            {
                LogMessage("", Color.White);
                LogMessage("→ Aplicando protección multicapa EOF...", Color.Yellow);
                IntegrityProtection.ApplyMultiLayerProtection(outputPath, data);
                LogMessage("  ✓ Protección multicapa EOF aplicada", Color.LightGreen);
                LogMessage("  ✓ Marcadores EOF inyectados", Color.LightGreen);
            }
        }

        private void LogMessage(string message, Color color)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() => LogMessage(message, color)));
                return;
            }

            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;
            logTextBox.SelectionColor = color;
            logTextBox.AppendText(message + Environment.NewLine);
            logTextBox.ScrollToCaret();
        }
    }
}