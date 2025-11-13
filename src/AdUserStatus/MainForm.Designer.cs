using System.Drawing;
using System.Windows.Forms;

namespace AdUserStatus
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            textBox1 = new TextBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            buttonHelp = new Button();
            tabResults = new TabControl();
            tabEnabled = new TabPage();
            gridEnabled = new DataGridView();
            tabDisabled = new TabPage();
            gridDisabled = new DataGridView();
            tabNotFound = new TabPage();
            gridNotFound = new DataGridView();
            tabExternal = new TabPage();
            gridExternal = new DataGridView();
            tabIcons = new ImageList(components);
            label1 = new Label();
            panelBottom = new Panel();
            tabResults.SuspendLayout();
            tabEnabled.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridEnabled).BeginInit();
            tabDisabled.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridDisabled).BeginInit();
            tabNotFound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridNotFound).BeginInit();
            tabExternal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridExternal).BeginInit();
            SuspendLayout();
            
            // label1
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 29);
            label1.Name = "label1";
            //label1.Size = new Size(71, 17);
            label1.TabIndex = 10;
            label1.Text = "Input File:";

            // textBox1
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(80, 28);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(525, 23);
            textBox1.TabIndex = 0;
            //textBox1.TextChanged += this.textBox1_TextChanged;

            // contextMenuStrip1
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);

            // button1
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(614, 27);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 3;
            button1.Text = "Browse";
            //button1.UseVisualStyleBackColor = true;

            // buttonHelp (Help)
            buttonHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonHelp.Location = new Point(button1.Right + 8, button1.Top);
            buttonHelp.Name = "buttonHelp";
            buttonHelp.Size = new Size(75, 23);
            buttonHelp.Text = "Help";
            buttonHelp.UseVisualStyleBackColor = true;

            // button2
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button2.Location = new Point(12, 505);
            button2.Name = "button2";
            button2.Size = new Size(85, 23);
            button2.TabIndex = 6;
            button2.Text = "Check User";
            //button2.UseVisualStyleBackColor = true;

            // button3
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button3.Location = new Point(344, 505);
            button3.Name = "button3";
            button3.Size = new Size(101, 23);
            button3.TabIndex = 7;
            button3.Text = "Export Results";
            //button3.UseVisualStyleBackColor = true;

            // button4
            button4.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button4.Location = new Point(697, 505);
            button4.Name = "button4";
            button4.Size = new Size(75, 23);
            button4.TabIndex = 8;
            button4.Text = "Exit";
            //button4.UseVisualStyleBackColor = true;

            // tabResults
            tabResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabResults.Controls.Add(tabEnabled);
            tabResults.Controls.Add(tabDisabled);
            tabResults.Controls.Add(tabNotFound);
            tabResults.Controls.Add(tabExternal);
            tabResults.ImageList = tabIcons;
            tabResults.ImeMode = ImeMode.KatakanaHalf;
            tabResults.Location = new Point(12, 75);
            tabResults.Name = "tabResults";
            tabResults.Padding = new Point(10, 5);
            tabResults.Size = new Size(764, 420);
            tabResults.SelectedIndex = 0;
            tabResults.TabIndex = 9;
            tabResults.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);

            // tabEnabled
            tabEnabled.Controls.Add(gridEnabled);
            tabEnabled.ImageKey = "Enabled";
            tabEnabled.Location = new Point(4, 35);
            tabEnabled.Name = "tabEnabled";
            tabEnabled.Padding = new Padding(3);
            tabEnabled.TabIndex = 0;
            tabEnabled.Text = "Enabled ";
            tabEnabled.UseVisualStyleBackColor = true;

            // gridEnabled
            gridEnabled.BackgroundColor = SystemColors.ControlLight;
            gridEnabled.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridEnabled.Dock = DockStyle.Fill;
            gridEnabled.Location = new Point(3, 3);
            gridEnabled.Name = "gridEnabled";
            gridEnabled.TabIndex = 5;

            // tabDisabled
            tabDisabled.Controls.Add(gridDisabled);
            tabDisabled.ImageKey = "Disabled";
            tabDisabled.Location = new Point(4, 35);
            tabDisabled.Name = "tabDisabled";
            tabDisabled.Padding = new Padding(3);
            tabDisabled.TabIndex = 1;
            tabDisabled.Text = "Disabled ";
            tabDisabled.UseVisualStyleBackColor = true;

            // gridDisabled
            gridDisabled.BackgroundColor = SystemColors.ControlLight;
            gridDisabled.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridDisabled.Dock = DockStyle.Fill;
            gridDisabled.Location = new Point(3, 3);
            gridDisabled.Name = "gridDisabled";
            gridDisabled.TabIndex = 0;

            // tabNotFound
            tabNotFound.Controls.Add(gridNotFound);
            tabNotFound.ImageKey = "NotFound";
            tabNotFound.Location = new Point(4, 35);
            tabNotFound.Name = "tabNotFound";
            tabNotFound.Padding = new Padding(3);
            tabNotFound.TabIndex = 2;
            tabNotFound.Text = "NotFound ";
            tabNotFound.UseVisualStyleBackColor = true;

            // gridNotFound
            gridNotFound.BackgroundColor = SystemColors.ControlLight;
            gridNotFound.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridNotFound.Dock = DockStyle.Fill;
            gridNotFound.Location = new Point(3, 3);
            gridNotFound.Name = "gridNotFound";
            gridNotFound.TabIndex = 0;

            // tabExternal
            tabExternal.Controls.Add(gridExternal);
            tabExternal.ImageKey = "External";
            tabExternal.Location = new Point(4, 35);
            tabExternal.Name = "tabExternal";
            tabExternal.Padding = new Padding(3);
            tabExternal.TabIndex = 3;
            tabExternal.Text = "External ";
            tabExternal.UseVisualStyleBackColor = true;

            // gridExternal
            gridExternal.BackgroundColor = SystemColors.ControlLight;
            gridExternal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridExternal.Dock = DockStyle.Fill;
            gridExternal.Location = new Point(3, 3);
            gridExternal.Name = "gridExternal";
            gridExternal.TabIndex = 0;

            // tabIcons
            tabIcons.ColorDepth = ColorDepth.Depth32Bit;
            tabIcons.ImageSize = new Size(24, 24);
            tabIcons.TransparentColor = Color.Transparent;

            tabIcons.Images.Add("Enabled", Properties.Resources.Enabled_24px);
            tabIcons.Images.Add("Disabled", Properties.Resources.Disabled_24px);
            tabIcons.Images.Add("NotFound", Properties.Resources.NotFound_24px);
            tabIcons.Images.Add("External", Properties.Resources.External_24px);

            // panelBottom
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Height = 50;               // space for buttons
            panelBottom.Padding = new Padding(8);
            panelBottom.BackColor = SystemColors.Control;

            // move buttons to panelBottom
            panelBottom.Controls.Add(button2);
            panelBottom.Controls.Add(button3);
            panelBottom.Controls.Add(button4);

            // Reposition for panel-relative coords
            button2.Location = new Point(8, 12);               // left
            button3.Location = new Point(8 + 85 + 8, 12);      // next to button2

            // Exit aligned to the right inside the panel
            button4.Size = new Size(75, 23);
            panelBottom.Resize += (_, __) =>
            {
                button4.Top = 12;
                button4.Left = panelBottom.ClientSize.Width - button4.Width - 8;
            };
            // do initial position
            button4.Top = 12;
            button4.Left = panelBottom.ClientSize.Width - button4.Width - 8;

            // MainForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(label1);
            Controls.Add(tabResults);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(buttonHelp);
            Controls.Add(panelBottom);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AD User Status";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)gridEnabled).EndInit();
            tabResults.ResumeLayout(false);
            tabEnabled.ResumeLayout(false);
            tabDisabled.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridDisabled).EndInit();
            tabNotFound.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridNotFound).EndInit();
            tabExternal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridExternal).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private ContextMenuStrip contextMenuStrip1;
        private Button button1;
        private DataGridView gridEnabled;
        private Button button2;
        private Button button3;
        private Button button4;
        private TabControl tabResults;
        private TabPage tabEnabled;
        private TabPage tabDisabled;
        private TabPage tabNotFound;
        private TabPage tabExternal;
        private DataGridView gridDisabled;
        private DataGridView gridNotFound;
        private DataGridView gridExternal;
        private Label label1;
        private ImageList tabIcons;
        private Button buttonHelp;
        private Panel panelBottom;
    }
}
