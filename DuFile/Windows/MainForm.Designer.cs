namespace DuFile.Windows
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
			menuStrip = new MenuStrip();
			toolStrip = new ToolStrip();
			leftPanel = new DuFile.Windows.FilePanel();
			verticalBar = new DuFile.Windows.VerticalBar();
			rightPanel = new DuFile.Windows.FilePanel();
			funcBar = new DuFile.Windows.FuncBar();
			SuspendLayout();
			// 
			// menuStrip
			// 
			menuStrip.Location = new Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.Size = new Size(800, 24);
			menuStrip.TabIndex = 0;
			menuStrip.Text = "menuStrip1";
			// 
			// toolStrip
			// 
			toolStrip.Location = new Point(0, 24);
			toolStrip.Name = "toolStrip";
			toolStrip.Size = new Size(800, 25);
			toolStrip.TabIndex = 1;
			toolStrip.Text = "toolStrip";
			toolStrip.Visible = false;
			// 
			// leftPanel
			// 
			leftPanel.BackColor = Color.FromArgb(37, 37, 37);
			leftPanel.ForeColor = Color.FromArgb(241, 241, 241);
			leftPanel.Location = new Point(0, 24);
			leftPanel.Name = "leftPanel";
			leftPanel.PanelIndex = 1;
			leftPanel.Size = new Size(378, 401);
			leftPanel.TabIndex = 2;
			leftPanel.Load += leftPanel_Load;
			// 
			// verticalBar
			// 
			verticalBar.BackColor = Color.FromArgb(37, 37, 37);
			verticalBar.ForeColor = Color.FromArgb(241, 241, 241);
			verticalBar.Location = new Point(384, 24);
			verticalBar.Name = "verticalBar";
			verticalBar.Size = new Size(20, 401);
			verticalBar.TabIndex = 3;
			// 
			// rightPanel
			// 
			rightPanel.BackColor = Color.FromArgb(37, 37, 37);
			rightPanel.ForeColor = Color.FromArgb(241, 241, 241);
			rightPanel.Location = new Point(420, 24);
			rightPanel.Name = "rightPanel";
			rightPanel.PanelIndex = 2;
			rightPanel.Size = new Size(380, 401);
			rightPanel.TabIndex = 4;
			rightPanel.Load += rightPanel_Load;
			// 
			// funcBar
			// 
			funcBar.BackColor = Color.FromArgb(63, 63, 70);
			funcBar.ButtonHeight = 25;
			funcBar.Dock = DockStyle.Bottom;
			funcBar.ForeColor = Color.FromArgb(241, 241, 241);
			funcBar.Location = new Point(0, 425);
			funcBar.Name = "funcBar";
			funcBar.Size = new Size(800, 25);
			funcBar.TabIndex = 5;
			funcBar.ButtonClicked += funcBar_ButtonClicked;
			// 
			// MainForm
			// 
			AllowDrop = true;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(funcBar);
			Controls.Add(rightPanel);
			Controls.Add(verticalBar);
			Controls.Add(leftPanel);
			Controls.Add(toolStrip);
			Controls.Add(menuStrip);
			KeyPreview = true;
			MainMenuStrip = menuStrip;
			MinimumSize = new Size(800, 450);
			Name = "MainForm";
			Text = "두파일";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private MenuStrip menuStrip;
		private ToolStrip toolStrip;
		private Windows.FilePanel leftPanel;
		private Windows.VerticalBar verticalBar;
		private Windows.FilePanel rightPanel;
		private Windows.FuncBar funcBar;
	}
}
