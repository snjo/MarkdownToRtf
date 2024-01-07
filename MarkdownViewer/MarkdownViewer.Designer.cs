namespace MarkdownViewer
{
    partial class MarkdownViewer
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
            buttonLoad = new Button();
            buttonSave = new Button();
            richTextBoxRtfView = new RichTextBox();
            button1 = new Button();
            richTextBoxRtfCode = new RichTextBox();
            panel1 = new Panel();
            buttonSaveMd = new Button();
            checkBoxLiveUpdate = new CheckBox();
            checkBoxShowRtfCode = new CheckBox();
            checkBoxShowSourceMd = new CheckBox();
            splitContainer1 = new SplitContainer();
            textBoxSourceMd = new TextBox();
            splitContainer2 = new SplitContainer();
            timerUpdate = new System.Windows.Forms.Timer(components);
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // buttonLoad
            // 
            buttonLoad.Location = new Point(3, 3);
            buttonLoad.Name = "buttonLoad";
            buttonLoad.Size = new Size(75, 23);
            buttonLoad.TabIndex = 0;
            buttonLoad.Text = "Load";
            buttonLoad.UseVisualStyleBackColor = true;
            buttonLoad.Click += ButtonLoad_Click;
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(286, 3);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 1;
            buttonSave.Text = "Save RTF";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // richTextBoxRtfView
            // 
            richTextBoxRtfView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxRtfView.Location = new Point(0, 5);
            richTextBoxRtfView.Name = "richTextBoxRtfView";
            richTextBoxRtfView.ReadOnly = true;
            richTextBoxRtfView.Size = new Size(542, 710);
            richTextBoxRtfView.TabIndex = 3;
            richTextBoxRtfView.Text = "";
            // 
            // button1
            // 
            button1.Location = new Point(84, 3);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "Update";
            button1.UseVisualStyleBackColor = true;
            button1.Click += buttonRefresh_Click;
            // 
            // richTextBoxRtfCode
            // 
            richTextBoxRtfCode.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxRtfCode.Location = new Point(0, 5);
            richTextBoxRtfCode.Name = "richTextBoxRtfCode";
            richTextBoxRtfCode.ReadOnly = true;
            richTextBoxRtfCode.Size = new Size(375, 710);
            richTextBoxRtfCode.TabIndex = 6;
            richTextBoxRtfCode.Text = "";
            // 
            // panel1
            // 
            panel1.Controls.Add(buttonSaveMd);
            panel1.Controls.Add(checkBoxLiveUpdate);
            panel1.Controls.Add(checkBoxShowRtfCode);
            panel1.Controls.Add(checkBoxShowSourceMd);
            panel1.Controls.Add(buttonLoad);
            panel1.Controls.Add(buttonSave);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1244, 30);
            panel1.TabIndex = 8;
            // 
            // buttonSaveMd
            // 
            buttonSaveMd.Location = new Point(165, 3);
            buttonSaveMd.Name = "buttonSaveMd";
            buttonSaveMd.Size = new Size(115, 23);
            buttonSaveMd.TabIndex = 8;
            buttonSaveMd.Text = "Save Markdown";
            buttonSaveMd.UseVisualStyleBackColor = true;
            buttonSaveMd.Click += buttonSaveMd_Click;
            // 
            // checkBoxLiveUpdate
            // 
            checkBoxLiveUpdate.AutoSize = true;
            checkBoxLiveUpdate.Checked = true;
            checkBoxLiveUpdate.CheckState = CheckState.Checked;
            checkBoxLiveUpdate.Location = new Point(599, 6);
            checkBoxLiveUpdate.Name = "checkBoxLiveUpdate";
            checkBoxLiveUpdate.Size = new Size(87, 19);
            checkBoxLiveUpdate.TabIndex = 7;
            checkBoxLiveUpdate.Text = "Live update";
            checkBoxLiveUpdate.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowRtfCode
            // 
            checkBoxShowRtfCode.AutoSize = true;
            checkBoxShowRtfCode.Checked = true;
            checkBoxShowRtfCode.CheckState = CheckState.Checked;
            checkBoxShowRtfCode.Location = new Point(488, 6);
            checkBoxShowRtfCode.Name = "checkBoxShowRtfCode";
            checkBoxShowRtfCode.Size = new Size(105, 19);
            checkBoxShowRtfCode.TabIndex = 6;
            checkBoxShowRtfCode.Text = "Show RTF code";
            checkBoxShowRtfCode.UseVisualStyleBackColor = true;
            checkBoxShowRtfCode.CheckedChanged += checkBoxShowRtfCode_CheckedChanged;
            // 
            // checkBoxShowSourceMd
            // 
            checkBoxShowSourceMd.AutoSize = true;
            checkBoxShowSourceMd.Checked = true;
            checkBoxShowSourceMd.CheckState = CheckState.Checked;
            checkBoxShowSourceMd.Location = new Point(367, 6);
            checkBoxShowSourceMd.Name = "checkBoxShowSourceMd";
            checkBoxShowSourceMd.Size = new Size(115, 19);
            checkBoxShowSourceMd.TabIndex = 5;
            checkBoxShowSourceMd.Text = "Show source MD";
            checkBoxShowSourceMd.UseVisualStyleBackColor = true;
            checkBoxShowSourceMd.CheckedChanged += checkBoxShowSourceMd_CheckedChanged;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(textBoxSourceMd);
            splitContainer1.Panel1MinSize = 0;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(richTextBoxRtfView);
            splitContainer1.Panel2MinSize = 100;
            splitContainer1.Size = new Size(862, 715);
            splitContainer1.SplitterDistance = 316;
            splitContainer1.TabIndex = 10;
            // 
            // textBoxSourceMd
            // 
            textBoxSourceMd.AcceptsTab = true;
            textBoxSourceMd.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSourceMd.Location = new Point(3, 5);
            textBoxSourceMd.Multiline = true;
            textBoxSourceMd.Name = "textBoxSourceMd";
            textBoxSourceMd.ScrollBars = ScrollBars.Both;
            textBoxSourceMd.Size = new Size(310, 710);
            textBoxSourceMd.TabIndex = 6;
            textBoxSourceMd.TextChanged += textBoxSourceMd_TextChanged;
            // 
            // splitContainer2
            // 
            splitContainer2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer2.Location = new Point(3, 31);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(splitContainer1);
            splitContainer2.Panel1MinSize = 100;
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(richTextBoxRtfCode);
            splitContainer2.Panel2MinSize = 0;
            splitContainer2.Size = new Size(1241, 715);
            splitContainer2.SplitterDistance = 862;
            splitContainer2.TabIndex = 7;
            // 
            // timerUpdate
            // 
            timerUpdate.Interval = 1000;
            timerUpdate.Tick += timerUpdate_Tick;
            // 
            // MarkdownViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1244, 747);
            Controls.Add(splitContainer2);
            Controls.Add(panel1);
            Name = "MarkdownViewer";
            Text = "Markdown Viewer";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button buttonLoad;
        private Button buttonSave;
        private RichTextBox richTextBoxRtfView;
        private Button button1;
        private RichTextBox richTextBoxRtfCode;
        private Panel panel1;
        private CheckBox checkBoxShowRtfCode;
        private CheckBox checkBoxShowSourceMd;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private TextBox textBoxSourceMd;
        private CheckBox checkBoxLiveUpdate;
        private System.Windows.Forms.Timer timerUpdate;
        private Button buttonSaveMd;
    }
}
