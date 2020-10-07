using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PDFSignatureRepairWindows
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e) {}

        private void button_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string repairedFileName = Path.GetTempPath() + "repaired_pdf_" + unixTimestamp.ToString() + ".pdf";
                try
                {
                    string filePath = openFileDialog.FileName;
                    byte[] buffer = File.ReadAllBytes(filePath);
                    byte[] pattern = { 0x25, 0x25, 0x45, 0x4F, 0x46, 0x0A };

                    int position = Search(buffer, pattern);

                    if (position != -1)
                    {
                        position += 6;

                        Array.Resize<byte>(ref buffer, position);

                        using (FileStream fileStream = new FileStream(repairedFileName, FileMode.Create))
                        {
                            for (int i = 0; i < buffer.Length; i++)
                            {
                                fileStream.WriteByte(buffer[i]);
                            }

                            fileStream.Close();
                        }

                        Process myProcess = new Process();
                        myProcess.StartInfo.FileName = "AcroRd32.exe";
                        myProcess.StartInfo.Arguments = "/A \"page=1=OpenActions\" " + repairedFileName;
                        myProcess.Start();
                    }
                    else
                    {
                        MessageBox.Show("Selected file is not corrupted or does not contain a digital signature.");
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        Process.Start(repairedFileName);
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show($"General error.\n\nError message: {ex2.Message}\n\n" + $"Details:\n\n{ex2.StackTrace}");
                    }
                }
            }
        }

        private int Search(byte[] src, byte[] pattern)
        {
            int c = src.Length - pattern.Length + 1;
            int j;
            for (int i = 0; i < c; i++)
            {
                if (src[i] != pattern[0]) continue;
                for (j = pattern.Length - 1; j >= 1 && src[i + j] == pattern[j]; j--) ;
                if (j == 0) return i;
            }
            return -1;
        }
    }
}
