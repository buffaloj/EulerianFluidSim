namespace SimRunnerApp.Winforms
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
            this.speedBar = new System.Windows.Forms.TrackBar();
            this.checkPressure = new System.Windows.Forms.CheckBox();
            this.checkColorSmoke = new System.Windows.Forms.CheckBox();
            this.checkFlowLines = new System.Windows.Forms.CheckBox();
            this.fpsLabel = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.simView = new SimRunnerApp.Winforms.SimView();
            ((System.ComponentModel.ISupportInitialize)(this.speedBar)).BeginInit();
            this.SuspendLayout();
            // 
            // speedBar
            // 
            this.speedBar.Location = new System.Drawing.Point(26, 20);
            this.speedBar.Maximum = 15000;
            this.speedBar.Name = "speedBar";
            this.speedBar.Size = new System.Drawing.Size(226, 45);
            this.speedBar.TabIndex = 0;
            this.speedBar.Scroll += new System.EventHandler(this.speedBar_Scroll);
            // 
            // checkPressure
            // 
            this.checkPressure.AutoSize = true;
            this.checkPressure.Location = new System.Drawing.Point(292, 29);
            this.checkPressure.Name = "checkPressure";
            this.checkPressure.Size = new System.Drawing.Size(70, 19);
            this.checkPressure.TabIndex = 1;
            this.checkPressure.Text = "Pressure";
            this.checkPressure.UseVisualStyleBackColor = true;
            this.checkPressure.CheckedChanged += new System.EventHandler(this.checkPressure_CheckedChanged);
            // 
            // checkColorSmoke
            // 
            this.checkColorSmoke.AutoSize = true;
            this.checkColorSmoke.Location = new System.Drawing.Point(379, 30);
            this.checkColorSmoke.Name = "checkColorSmoke";
            this.checkColorSmoke.Size = new System.Drawing.Size(94, 19);
            this.checkColorSmoke.TabIndex = 2;
            this.checkColorSmoke.Text = "Color Smoke";
            this.checkColorSmoke.UseVisualStyleBackColor = true;
            this.checkColorSmoke.CheckedChanged += new System.EventHandler(this.checkColorSmoke_CheckedChanged);
            // 
            // checkFlowLines
            // 
            this.checkFlowLines.AutoSize = true;
            this.checkFlowLines.Location = new System.Drawing.Point(493, 30);
            this.checkFlowLines.Name = "checkFlowLines";
            this.checkFlowLines.Size = new System.Drawing.Size(109, 19);
            this.checkFlowLines.TabIndex = 3;
            this.checkFlowLines.Text = "Slow Flow Lines";
            this.checkFlowLines.UseVisualStyleBackColor = true;
            this.checkFlowLines.CheckedChanged += new System.EventHandler(this.checkFlowLines_CheckedChanged);
            // 
            // fpsLabel
            // 
            this.fpsLabel.AutoSize = true;
            this.fpsLabel.Location = new System.Drawing.Point(624, 31);
            this.fpsLabel.Name = "fpsLabel";
            this.fpsLabel.Size = new System.Drawing.Size(29, 15);
            this.fpsLabel.TabIndex = 4;
            this.fpsLabel.Text = "0fps";
            // 
            // simView
            // 
            this.simView.Location = new System.Drawing.Point(0, 71);
            this.simView.Name = "simView";
            this.simView.Size = new System.Drawing.Size(800, 379);
            this.simView.TabIndex = 6;
            this.simView.Text = "simView1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.simView);
            this.Controls.Add(this.fpsLabel);
            this.Controls.Add(this.checkFlowLines);
            this.Controls.Add(this.checkColorSmoke);
            this.Controls.Add(this.checkPressure);
            this.Controls.Add(this.speedBar);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.speedBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TrackBar speedBar;
        private CheckBox checkPressure;
        private CheckBox checkColorSmoke;
        private CheckBox checkFlowLines;
        private Label fpsLabel;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private SimView simView;
    }
}