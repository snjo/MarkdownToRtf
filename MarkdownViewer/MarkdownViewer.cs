using MarkdownToRtf;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace MarkdownViewer
{
    public partial class MarkdownViewer : Form
    {
        string rtfText = string.Empty;
        string FileName = "readme.MD";
        //string testFile = "test.md";
        bool errorPopup = false;

        public MarkdownViewer(string[] args)
        {
            InitializeComponent();
            UpdateSplitters();
            ProcessArguments(args);
            OpenFile(FileName);
        }

        private void ProcessArguments(string[] args)
        {
            if (args.Length == 0) return;
            string file = args[0];
            if (File.Exists(file))
            {
                FileName = file;
                checkBoxShowRtfCode.Checked = false;
                //checkBoxShowSourceMd.Checked = false;
            }
        }

        public void OpenFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                Debug.WriteLine("Loading file: " + fileName);
                string text = File.ReadAllText(fileName, System.Text.Encoding.UTF8);
                textBoxSourceMd.Text = text;
                LoadText(text, FileName);
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

        private void LoadText(string text, string fileName)
        {
            RtfConverter rtfConverter = new(fileName);
            rtfText = rtfConverter.ConvertText(text);

            if (rtfConverter.Errors.Count > 0 && errorPopup)
            {
                string errors = LineListToString(rtfConverter.Errors);
                DialogResult result = MessageBox.Show(errors + "\n\n To stop showing errors, press No", "Parsing error", MessageBoxButtons.YesNo);
                if (result == DialogResult.No) { errorPopup = false; }
            }

            // MUST set Readonly to false, otherwise images will not load. This is a bug in RichTextBox.
            bool oldReadonly = richTextBoxRtfView.ReadOnly;
            richTextBoxRtfView.ReadOnly = false;
            richTextBoxRtfView.Rtf = rtfText;
            richTextBoxRtfView.ReadOnly = oldReadonly;

            // moved this to TextChanged event
            //richTextBoxRtfCode.Text = richTextBoxRtfView.Rtf;
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
                LoadText(textBoxSourceMd.Text, FileName);
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
            LoadText(textBoxSourceMd.Text, FileName);
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
            LoadText(textBoxSourceMd.Text, FileName);
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
            string? linkURL = e.LinkText;
            if (linkURL != null)
            {
                string docFolder = Path.GetDirectoryName(FileName) + "";
                string linkPath = Path.Combine(docFolder, linkURL);
                Debug.WriteLine($"Open file: {linkPath} (folder{docFolder}, linkURL{linkURL})");
                if (File.Exists(linkPath))
                {
                    try
                    {
                        OpenFileExternal(linkPath);
                    }
                    catch
                    {
                        Debug.WriteLine("Exception when opening file: " + linkPath);
                    }

                }
                else
                {
                    try
                    {
                        OpenLink(linkURL);
                    }
                    catch
                    {
                        Debug.WriteLine("Exception when opening link: " + linkURL);
                    }
                }
            }
            else
            {
                Debug.WriteLine("Link is null");
            }

        }

        public static void OpenLink(string url)
        {
            Process.Start(new ProcessStartInfo() { FileName = url, UseShellExecute = true });
        }

        private void OpenFileExternal(string file)
        {
            if (File.Exists(file))
            {
                Process.Start(new ProcessStartInfo() { FileName = file, UseShellExecute = true });
            }
            else
            {
                Debug.WriteLine("Can't open file, not found: " + file);
            }
        }

        private void richTextBoxRtfView_TextChanged(object sender, EventArgs e)
        {
            richTextBoxRtfCode.Text = richTextBoxRtfView.Rtf;
        }
    }
}
