using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
namespace PhimmoiGetlink
{
    public partial class Form1 : Form
    {
        private CoreProcessor processor;
        private List<Process> processes;
        private List<int> fileSizes;
        private List<string> percentages;
        public Form1()
        {
            InitializeComponent();
            processor = CoreProcessor.GetInstance();
            tbSaveTo.Text = "D:\\Movies\\output.mp4";
            saveFileDialog1.CheckFileExists = false;
            fileSizes = new List<int>();
            processes = new List<Process>();
            percentages = new List<string>();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Uri outUri;

            if (Uri.TryCreate(tbURL.Text, UriKind.Absolute, out outUri)
               && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            {
                if (processes.Count > 0)
                {
                    var  i = 999999999;
                }
                //Lấy link HLS
                var hlsURL = processor.GetHLSURL(tbURL.Text);
                if (hlsURL == "Nope")
                    return;

                //Thêm link vô danh sách
                listBox1.Items.Add(tbURL.Text);
                fileSizes.Add(0);
                percentages.Add("0%");

                //Tiến hành download
                Process process = new Process();
                processes.Add(process);
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.FileName = "ffmpeg.exe";

                process.StartInfo.Arguments = " -y -i " + hlsURL + " -c copy -bsf:a aac_adtstoasc \"" + tbSaveTo.Text + "\""; ;

                process.OutputDataReceived += CaptureOutput;
                process.ErrorDataReceived += CaptureError;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                ThreadStart ths = new ThreadStart(() => { process.Start(); process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                });

                Thread th = new Thread(ths);
                th.Start();
 
                
                

            }

        }

        private string ExtractTextBetween(string source, string begin, string end)
        {
            if (!source.Contains(begin) || !source.Contains(end))
                return "Nope";
            int i1 = source.IndexOf(begin);
            int i2 = source.IndexOf(end);
            return source.Substring(i1 + begin.Length, i2 - (i1 + begin.Length) ).Trim();

        }

        private void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            
               // Debug.WriteLine(e.Data);
        }

        private void CaptureError(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            int currentProcessIndex = processes.FindIndex(p => p == sender);
            //Kiem thong tin dung luong file
            if (fileSizes[currentProcessIndex] == 0)
                if (e.Data.Contains("Duration:"))
                {
                    string durationRaw = ExtractTextBetween(e.Data, "Duration:", ", start");
                    if (durationRaw == "Nope")
                        return;
                    var durations = durationRaw.Split(':');
                    int h = int.Parse(durations[0]);
                    int m = int.Parse(durations[1]);
                    int s = int.Parse(durations[2].Split('.')[0]);
                    fileSizes[currentProcessIndex] = h * 3600 + m*60 + s;
                }

            //Kiểm tra dung lượng file đã download
            if (e.Data.Contains("time"))
            {
                string durationRaw = ExtractTextBetween(e.Data, "time=", "bitrate=");
                if (durationRaw == "Nope")
                    return;
                var durations = durationRaw.Split(':');
                int h = int.Parse(durations[0]);
                int m = int.Parse(durations[1]);
                int s = int.Parse(durations[2].Split('.')[0]);
                int minutes = h * 3600 + 60*m + s;
                if (fileSizes[currentProcessIndex] == 0)
                    return;
                if(minutes == 1)
                {
                    //Debug
                    var i = 0;
                }
                percentages[currentProcessIndex] = ((float)(minutes * 100 )/ fileSizes[currentProcessIndex]).ToString() + "%";
                MethodInvoker inv = delegate
                {
                    listBox2.DataSource = null;
                    listBox2.DataSource = percentages;
                    listBox2.Refresh();

                };
                this.Invoke(inv);
                
            }
            Debug.WriteLine(e.Data);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.tbSaveTo.Text = saveFileDialog1.FileName;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            processor.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
        }
    }
}
