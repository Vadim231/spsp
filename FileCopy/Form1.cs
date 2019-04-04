using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
      btnResume.Enabled = false;
    }

    private void btnSourceFile_Click(object sender, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.CheckFileExists = true;
      dlg.Multiselect = false;
      dlg.Title = "Открытие файла";
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        txtSourceFile.Text = dlg.FileName;
      }
    }
    private void btnDestFile_Click(object sender, EventArgs e)
    {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.CheckFileExists = false;
      dlg.OverwritePrompt = true;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        txtDestFile.Text = dlg.FileName;
      }
      else txtDestFile.Text = "";
    }

    Thread thCopyFile = null;
    CopyParam paramCopyFile = null;

    private void btnStart_Click(object sender, EventArgs e)
    {
      if (txtSourceFile.Text.Trim().Length == 0 ||
         txtDestFile.Text.Trim().Length == 0)
      {
        MessageBox.Show("Файл не указан");
        return;
      }
      if (thCopyFile != null)
      {
        return;
      }
      thCopyFile = new Thread(ThCopyRoutine);
      thCopyFile.IsBackground = true;
      paramCopyFile = new CopyParam();

      paramCopyFile.srcFileName =
                      txtSourceFile.Text.Trim();
      paramCopyFile.destFileName =
                      txtDestFile.Text.Trim();

      paramCopyFile.frm = this;
      pbFileCopy.Value = 0;
      pbFileCopy.Minimum = 0; 
      pbFileCopy.Maximum = 1000; 
      pbFileCopy.Step = 100; 

      thCopyFile.Start(paramCopyFile);
    }

    int readSize = 2*1024; 

    void ThCopyRoutine(object arg)
    {
      CopyParam par = arg as CopyParam;
      FileStream src =
        new FileStream(par.srcFileName,
                       FileMode.Open,
                       FileAccess.Read);
      FileStream dst = 
        new FileStream(par.destFileName,
                       FileMode.OpenOrCreate,
                       FileAccess.Write);

      byte[] buf = new byte[readSize];
            

      FileInfo fi = new FileInfo(par.srcFileName);
      long fileSize = fi.Length;
      long readAll = 0;
      while (!par.IsStop)
      {
        int readBytes = src.Read(buf, 0, readSize);
        dst.Write(buf, 0, readBytes);

        readAll += readBytes;

        int readProcent =
    (int)((double)readAll / fileSize * 100.0 * 10 + 0.5);
        par.frm.pbFileCopy.Invoke(new Action<int>(
          (x) => { par.frm.pbFileCopy.Value = x;
            par.frm.pbFileCopy.Update();
          }),
          readProcent);
                
        if(readAll == fileSize) {
          par.IsStop = true;
        }
        par.evnPause.WaitOne();
      }
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
            Stop();
    }

        void Stop()
        {
            btnResume.Enabled = true;
            paramCopyFile.evnPause.Reset();
            paramCopyFile.IsStop = true;
        }

    private void btnResume_Click(object sender, EventArgs e)
    {
            Resume();
    }

        void Resume()
        {
            paramCopyFile.evnPause.Set();
            paramCopyFile.IsStop = false;
            btnResume.Enabled = false;
        }

    private void btnClose_Click(object sender, EventArgs e)
    {
            if (paramCopyFile == null) { Application.Exit();return; }
            Stop();
            var result = MessageBox.Show("Вы уверены, что хотите прекратить работу программы?", "Упс", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
            else
            {
                Resume();
            }
    }
  }
}
