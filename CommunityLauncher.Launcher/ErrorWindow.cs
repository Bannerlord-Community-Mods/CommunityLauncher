using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !NETSTANDARD
using System.Windows.Forms;
#endif

namespace CommunityLauncher.Launcher
{
#if !NETSTANDARD
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class ErrorWindow : Form
    {
        private WebBrowser widget = new WebBrowser();
        private Button button1 = new Button();
        private string errorString;
        private string faultingSource;
        private string fullStackString;
        public static void Display(string err, string fault, string stack)
        {
            var x = new ErrorWindow(err, fault, stack);
            x.ShowDialog();
            Console.WriteLine("Error at module: " + fault);
            Console.WriteLine("Reason :" + err);
            Console.WriteLine(stack);
        }

        private ErrorWindow(string err, string fault, string stack)
        {
            button1.Text = "call script code from client code";
            button1.Dock = DockStyle.Top;
            widget.Dock = DockStyle.Fill;
            Controls.Add(widget);
            //Controls.Add(button1);
            errorString = err;
            faultingSource = fault;
            fullStackString = stack;
            this.Width = 900;
            this.Height = 500;
            this.TopMost = true;
            this.ShowIcon = false;            
            Load += new EventHandler(Form1_Load);
            button1.Click += new EventHandler(button1_Click);
        }

        protected override void OnClosed(EventArgs e)
        {
            CloseProgram();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            widget.IsWebBrowserContextMenuEnabled = false;
            widget.ContextMenu = null;
            widget.ContextMenuStrip = null;
            TopMost = true;
            widget.ObjectForScripting = this;
            var html = @"<html>
                       <head>
                           <style>
                            .button{
                                float:right;
                                margin-left:10px
                            }
                            .monospace {
                                font-family: 'Lucida Console', Courier, monospace;
                            }
                            #title{
                                float:left
                            }
                            .msg{
                                display: none
                            }
                            #dsc {
                                padding-top: 0px;
                                padding-bottom: 5px;
                            }
                            img {
                              float: left;
                            }
                            </style>
                       </head>
                       <body>
                           <div style='clear both;'>
                               <h1 id='title'> An unhandled exception occured! </h1>
                               <button class='button' onclick='window.external.CloseProgram()'>
                                    Close program
                                </button>
                               <button class='button' onclick='window.external.Save()'>
                                    Save this page
                                </button>
                           </div>
                           <br/>
                           <br/>
                           <hr/>
                           <table style='width: 100 %; '>
                            <tbody>
                               <tr>
                                   <td style='width: 8 %; '> 
                                       <img src='http://icons.iconarchive.com/icons/paomedia/small-n-flat/256/sign-error-icon.png' width='64' height='64'/>
                                   </td>
                                   <td>
                                       <p><b>Bannerlord has encountered a problem and needs to close. We are sorry for the inconvience. </b><br/>
                                    If you were in the middle of something, the progress you were working on might be lost.<br/>
                                    This error can be caused by a faulty module XMLs, manifest (submodule.xml), or DLL (bad one or permission error).</p>
                                   </td>
                               </tr>                            
                           </tbody>
                           </table>
                           <br/>
                           <h2>
                               <a href='#' onclick='showFaultingProcedure(this, ""Reasons"" ,""reason"")'>
                                + Show Reason
                            </a>
                           </h2>
                           <div class='msg' id='reason'>
                                Source: <span>{faultingSource}</span>
                               <p class='monospace' id='reasonPre'>
                                    {errorString}
                               </p>
                           </div>
                           <h2>
                               <a href='#' onclick='showFaultingProcedure(this, ""Full stacks"" ,""fullStack"")'>
                                + Show Full stacks
                                </a>
                           </h2>
                           <div class='msg' id='fullStack'>
                               <p><b>Protip: </b>
                                Use a debugger like <a href='http://github.com/0xd4d/dnSpy'>dnSpy</a> or 
                                <a href='http://visualstudio.microsoft.com/'>Visual studio</a> to trace the source of error, by stepping the program
                                line by line.
                                </p>
<pre id='fullStackPre'>{fullStackString}</pre>
                           </div>
                           <script>                                                        
                            function showFaultingProcedure(e, msg, el)
                            {
                                if(document.getElementById(el).style.display == 'block') {
                                    document.getElementById(el).style.display = 'none';
                                    e.innerHTML = '+ Show ' + msg
                                }
                                else
                                {
                                    document.getElementById(el).style.display = 'block';
                                    e.innerHTML = '- Hide ' + msg
                                }
                            }
                            </script>
                       </body>
                   </html>";
            html = html.Replace("{errorString}", errorString);
            html = html.Replace("{faultingSource}", faultingSource);
            html = html.Replace("{fullStackString}", fullStackString);
            widget.DocumentText = html;
        }
        public void CloseProgram()
        {
            var pid = Process.GetCurrentProcess().Id;
            Process proc = Process.GetProcessById(pid);
            proc.Kill();
        }

        public void Save()
        {
            var filename = "../logs/" + DateTime.Now.ToFileTimeUtc().ToString() + ".htm";
            File.WriteAllText(filename, widget.DocumentText);
            var filePath = Path.GetFullPath(filename);
            MessageBox.Show(filePath, "Saved to");
        }

        private void widget_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            var isUri = Uri.IsWellFormedUriString(e.Url.ToString(), UriKind.RelativeOrAbsolute);
            if ((isUri && e.Url.ToString().StartsWith("http://")))
            {
                e.Cancel = true;
                try
                {
                    Process.Start(e.Url.ToString());
                }
                catch (Exception ex) {}
                return;
                try
                {
                    Process.Start("firefox.exe", e.Url.ToString());
                }
                catch (Exception ex) { }
                return;
                try
                {
                    Process.Start("chrome.exe", e.Url.ToString());
                }
                catch (Exception ex) { }
            }
        }

        public void Test(String message)
        {
            MessageBox.Show(message, "client code");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            widget.Document.InvokeScript("test",
                new String[] { "called from client code" });
        }

    }
#else
    public class ErrorWindow
    {
        public static void Display(string err, string fault, string stack)
        {            
            Console.WriteLine("Error at module: "+ fault);
            Console.WriteLine("Reason :" + err);
            Console.WriteLine(stack);            
        }
    }
#endif
}
