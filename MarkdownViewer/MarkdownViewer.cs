using MarkdownToRtf;
using System.Diagnostics;
using System.Text;
using static System.Windows.Forms.LinkLabel;

namespace MarkdownViewer
{
    public partial class MarkdownViewer : Form
    {
        string rtfText = string.Empty;
        string FileName = "readme.MD";
        string testFile = "test.md";
        bool errorPopup = true;

        public MarkdownViewer()
        {
            InitializeComponent();
            UpdateSplitters();
            OpenFile(FileName);
        }

        public void OpenFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                Debug.WriteLine("Loading file: " + fileName);
                string text = File.ReadAllText(fileName, System.Text.Encoding.UTF8);
                textBoxSourceMd.Text = text;
                LoadText(text);
            }
            else
            {
                Debug.WriteLine("Could not load file: " + fileName);
            }

            if (rtfText.Length == 0)
            {
                rtfText = "";
                Debug.WriteLine("RTF text is 0 long");
            }

        }

        private void LoadText(string text)
        {

            RtfConverter rtfConverter = new();
            rtfText = rtfConverter.ConvertText(text);

            if (rtfConverter.Errors.Count > 0 && errorPopup)
            {
                string errors = LineListToString(rtfConverter.Errors);
                DialogResult result = MessageBox.Show(errors + "\n\n To stop showing errors, press No", "Parsing error", MessageBoxButtons.YesNo);
                if (result == DialogResult.No) { errorPopup = false; }
            }

            richTextBoxRtfCode.Text = rtfText;
            richTextBoxRtfView.Rtf = rtfText;
        }

        private string LineListToString(List<string> lines)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        private void CopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtfText, TextDataFormat.Rtf);
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadText(textBoxSourceMd.Text);
            }
            if (e.KeyCode == Keys.O && e.Modifiers == Keys.Control)
            {
                OpenFileAction();
            }
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileAction();
        }

        private void OpenFileAction()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Markdown|*.md",
                ShowPinnedPlaces = true,
                ShowPreview = true
            };
            DialogResult = openFileDialog.ShowDialog();
            if (DialogResult == DialogResult.OK)
            {
                FileName = openFileDialog.FileName;
                OpenFile(FileName);
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            LoadText(textBoxSourceMd.Text);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string saveFile = "readme.rtf";
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Rich Text|*.rtf|All files|*.*",
                FileName = saveFile,
                OverwritePrompt = true
            };
            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                saveFile = saveFileDialog.FileName;
                File.WriteAllText(saveFile, rtfText); // DON'T specify UTF-8 encoding. It will add the byte markers at the front, making the file incompatible with Word/Wordpad
            }
        }

        private void checkBoxShowSourceMd_CheckedChanged(object sender, EventArgs e)
        {

            UpdateSplitters();
        }

        private void checkBoxShowRtfCode_CheckedChanged(object sender, EventArgs e)
        {

            UpdateSplitters();
        }

        private void UpdateSplitters()
        {
            textBoxSourceMd.Visible = checkBoxShowSourceMd.Checked;
            richTextBoxRtfCode.Visible = checkBoxShowRtfCode.Checked;
            if (checkBoxShowSourceMd.Checked)
            {
                splitContainer1.SplitterDistance = splitContainer1.Width / 2;
            }
            else
            {
                splitContainer1.SplitterDistance = 0;
            }

            if (checkBoxShowRtfCode.Checked)
            {
                splitContainer2.SplitterDistance = splitContainer2.Width - (splitContainer2.Width / 3);
            }
            else
            {
                splitContainer2.SplitterDistance = splitContainer2.Width;
            }
        }

        private void textBoxSourceMd_TextChanged(object sender, EventArgs e)
        {
            if (checkBoxLiveUpdate.Checked)
            {
                if (!timerUpdate.Enabled)
                {
                    //Debug.WriteLine("  Start timer");
                    timerUpdate.Start();
                }
                else
                {
                    //Debug.WriteLine("    Timer already running, restarting");
                    timerUpdate.Enabled = false;
                    timerUpdate.Start();

                }
            }
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("Timer tick, update text");
            LoadText(textBoxSourceMd.Text);
            timerUpdate.Stop();

        }

        private void buttonSaveMd_Click(object sender, EventArgs e)
        {
            string saveFile = FileName;
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Markdown|*.md|All files|*.*",
                FileName = saveFile,
                OverwritePrompt = true
            };
            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                saveFile = saveFileDialog.FileName;
                File.WriteAllText(saveFile, textBoxSourceMd.Text); // DON'T specify UTF-8 encoding. It will add the byte markers at the front
            }
        }

        private void richTextBoxRtfView_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Debug.WriteLine($"Link Clicked: {e.LinkText}, start: {e.LinkStart}, length{e.LinkLength}");
        }
    }
}
