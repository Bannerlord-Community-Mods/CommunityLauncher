namespace CommunityLauncher
{
    partial class ErrorWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
            this.widget = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // widget
            // 
            this.widget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.widget.Location = new System.Drawing.Point(0, 0);
            this.widget.MinimumSize = new System.Drawing.Size(20, 20);
            this.widget.Name = "widget";
            this.widget.Size = new System.Drawing.Size(800, 450);
            this.widget.TabIndex = 0;
            this.widget.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.widget_DocumentCompleted);
            this.widget.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.widget_Navigating);
            // 
            // ErrorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.widget);
            this.Name = "ErrorWindow";
            this.Text = "ErrorWindow";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ErrorWindow_FormClosed);
            this.Load += new System.EventHandler(this.ErrorWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser widget;
    }
}