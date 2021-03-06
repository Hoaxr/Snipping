﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace snippingtool
{


    public partial class MainForm : Form
    {
        private const string API_KEY = "830bececb56919ddd399ee27d45ffec4";
        private Imgur imgur;
        private const string COPIED_CLIPBOARD = "{0} link was copied to your clipboard";

        public void uploadProgressUpdateThreadSafe(snippingtool.Imgur.ProgressData data)
        {
            progressBar1.Maximum = data.max_value;
            progressBar1.Value = data.value;
        }

        public void uploadCompleteThreadSafe(snippingtool.Imgur.UploadResults results)
        {
            imgur_textbox.Text = results.imgur_page;
            original_textbox.Text = results.original;
            delete_textbox.Text = results.delete_page;

            Clipboard.SetText(results.imgur_page);
            copied_label.Text = String.Format(COPIED_CLIPBOARD, "Imgur");
            tabControl1.SelectedTab = tabPage2;
            ((Control)this.tabPage2).Enabled = true;
        }

        /// <summary>
        /// Gets called from another thread when the upload has a progress update
        /// </summary>
        /// <param name="data"></param>
        public void uploadProgressUpdate(snippingtool.Imgur.ProgressData data)
        {
            this.Invoke(new snippingtool.Imgur.uploadProgressUpdate(uploadProgressUpdateThreadSafe), new object[] { data });
        }

        /// <summary>
        /// Gets called from another thread when the upload has completed
        /// </summary>
        /// <param name="data"></param>
        public void uploadComplete(snippingtool.Imgur.UploadResults results)
        {
            this.Invoke(new snippingtool.Imgur.uploadComplete(uploadCompleteThreadSafe), new object[] { results });
        }

        public MainForm()
        {
            InitializeComponent();
            ((Control)this.tabPage2).Enabled = false;

            imgur = new Imgur(API_KEY);
            imgur.UploadProgressUpdateProperty = uploadProgressUpdate;
            imgur.UploadCompleteProperty = uploadComplete;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                this.Opacity = .0; // Hides dialog window while selecting 
                progressBar1.Value = 0;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var bmp = SnippingTool.Snip();
            this.Opacity = 1; // Shows dialog window while uploading
            if (bmp != null)
            {
                try
                {
                    imgur.PostImage(bmp, ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void OpenBrowser_click(object sender, EventArgs e)
        {
            Label box = (Label)sender;
            // load up the browser link

            switch (box.Text)
            { 
                case "Imgur":
                    Process.Start(imgur_textbox.Text);
                    break;
                case "Original":
                    Process.Start(original_textbox.Text);
                    break;
                case "Delete":
                    Process.Start(delete_textbox.Text);
                    break;
                default:
                    break;
            }
        }

        private void Copy_click(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;
            Clipboard.SetText(box.Text);
            
            switch (box.Name)
            { 
                case "imgur_textbox":
                    copied_label.Text = String.Format(COPIED_CLIPBOARD, "Imgur");
                    break;
                case "original_textbox":
                    copied_label.Text = String.Format(COPIED_CLIPBOARD, "Original");
                    break;
                case "delete_textbox":
                    copied_label.Text = String.Format(COPIED_CLIPBOARD, "Delete");
                    break;

                default:
                    break;
            }
        }

        private void toolStripStatusLabel1_Click_1(object sender, EventArgs e)
        {
            // Visit website
            System.Diagnostics.Process.Start("http://www.bdekker.nl");
        }
    }
}
