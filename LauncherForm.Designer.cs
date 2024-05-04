namespace OfflineMinecraftLauncher
{
    partial class LauncherForm
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LauncherForm));
            label1 = new Label();
            cbVersion = new ComboBox();
            pbFiles = new ProgressBar();
            pbProgress = new ProgressBar();
            lbProgress = new Label();
            btnStart = new Button();
            usernameInput = new TextBox();
            label2 = new Label();
            minecraftVersion = new ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 53);
            label1.Name = "label1";
            label1.Size = new Size(82, 15);
            label1.TabIndex = 0;
            label1.Text = "Select version:";
            // 
            // cbVersion
            // 
            cbVersion.FormattingEnabled = true;
            cbVersion.Location = new Point(12, 71);
            cbVersion.Name = "cbVersion";
            cbVersion.Size = new Size(230, 23);
            cbVersion.TabIndex = 1;
            // 
            // pbFiles
            // 
            pbFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pbFiles.Location = new Point(12, 148);
            pbFiles.Name = "pbFiles";
            pbFiles.Size = new Size(380, 20);
            pbFiles.TabIndex = 3;
            // 
            // pbProgress
            // 
            pbProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pbProgress.Location = new Point(12, 177);
            pbProgress.Name = "pbProgress";
            pbProgress.Size = new Size(380, 20);
            pbProgress.TabIndex = 4;
            // 
            // lbProgress
            // 
            lbProgress.AutoSize = true;
            lbProgress.Location = new Point(12, 130);
            lbProgress.Name = "lbProgress";
            lbProgress.Size = new Size(0, 15);
            lbProgress.TabIndex = 5;
            // 
            // btnStart
            // 
            btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnStart.Location = new Point(12, 100);
            btnStart.Margin = new Padding(0);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(380, 25);
            btnStart.TabIndex = 7;
            btnStart.Text = "Launch";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // usernameInput
            // 
            usernameInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            usernameInput.Location = new Point(12, 27);
            usernameInput.Name = "usernameInput";
            usernameInput.Size = new Size(380, 23);
            usernameInput.TabIndex = 9;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 8;
            label2.Text = "Username:";
            // 
            // minecraftVersion
            // 
            minecraftVersion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            minecraftVersion.FormattingEnabled = true;
            minecraftVersion.Items.AddRange(new object[] { "Releases and Installed", "All Versions" });
            minecraftVersion.Location = new Point(252, 71);
            minecraftVersion.Name = "minecraftVersion";
            minecraftVersion.Size = new Size(140, 23);
            minecraftVersion.TabIndex = 10;
            minecraftVersion.Text = "Releases and Installed";
            minecraftVersion.SelectedIndexChanged += minecraftVersion_SelectedIndexChanged;
            // 
            // LauncherForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(404, 211);
            Controls.Add(minecraftVersion);
            Controls.Add(usernameInput);
            Controls.Add(label2);
            Controls.Add(btnStart);
            Controls.Add(lbProgress);
            Controls.Add(pbProgress);
            Controls.Add(pbFiles);
            Controls.Add(cbVersion);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(420, 250);
            MinimumSize = new Size(420, 250);
            Name = "LauncherForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Offline Minecraft Launcher";
            Load += LauncherForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox cbVersion;
        private ProgressBar pbFiles;
        private ProgressBar pbProgress;
        private Label lbProgress;
        private Button btnStart;
        private ComboBox minecraftVersion;
        private TextBox usernameInput;
        private Label label2;
    }
}
