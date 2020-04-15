using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommunityLauncher
{
    [PermissionSet(SecurityAction.Demand, Name ="FullTrust")]
    [ComVisibleAttribute(true)]
    public partial class ErrorWindow : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        public static string errorString = "No errors :^)";
        public static string fullStackString = "No errors :^)";
        public static string faultingSource = "No module";

        public ErrorWindow()
        {
            InitializeComponent();
        }

        private void widget_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void ErrorWindow_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            var menu = new System.Windows.Forms.ContextMenu();
            menu.MenuItems.Add("none");
            widget.ContextMenu = menu;
            fullStackString = fullStackString.Replace(" at", "&nbsp; at");
            widget.ObjectForScripting = this;
            //maybe we should put it as .htm somewhere? idk
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
                            padding-top: 1px;
                            padding-bottom: 5px;
                        }
                        img {
                          float: left;
                        }
                    </style>               
                </head>";
            html +=
            $@"<body>
                    <div id='heading'>
                        <h1 id='title'> An unhandled exception occured! </h1>
                        <button class='button' onclick='window.external.CloseProgram()'>
                            Close program
                        </button>
                        <button class='button' onclick='window.external.Save()'>
                            Save this page
                        </button>
                    </div>   
                    <br />
                    <br />
                    <div><hr /></div>                    
                    <table style='width: 100 %; '>
                    <tbody>
                        <tr>
                            <td > 
                                <img src='http://icons.iconarchive.com/icons/paomedia/small-n-flat/256/sign-error-icon.png' width='64' height='64'/>
                            </td>
                            <td > 
                                <p id='dec'>
                                    <b>Bannerlord has encountered a problem and needs to close. We are sorry for the inconvience. </b><br/>
                                    If you were in the middle of something, the progress you were working on might be lost.<br/>
                                    This error can be caused by a faulty module XMLs, manifest (submodule.xml), or DLL (bad one or permission error).
                                </p>        
                            </td>
                        </tr>
                    </tbody>
                    </table>
                    </hr>
                    <br />
                    <h2>
                         <a href='#' onclick='showFaultingProcedure(this, ""Reasons"" ,""reason"")'>
                            + Show Reason
                         </a>
                    </h2>
                    <div class='msg' id='reason'>
                        Source: <span id='sourceName'>{faultingSource}</span>
                        <p class='monospace' id='reasonPre'>{errorString}
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
                            <a href='http://visualstudio.microsoft.com'>Visual studio</a> to trace the source of error, by stepping the program
                            line by line.
                        </p>
                        <pre class='monospace'  id='fullStackPre'>{fullStackString}</pre>
                    </div>
                    <script>";
            html += @"
                        //document.getElementById('faultingStack').innerHTML  = window.external.CallStack()
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
            widget.DocumentText = html;
        }

        public void Save()
        {
            var filename = DateTime.Now.ToFileTimeUtc().ToString() + ".htm";
            File.WriteAllText(filename, widget.DocumentText);
            var filePath = Path.GetFullPath(filename);
            MessageBox.Show(filePath, "Saved to");
        }

        public void CloseProgram()
        {
            var pid = Process.GetCurrentProcess().Id;
            var proc = Process.GetProcessById(pid);
            proc.Kill();
        }


        private void widget_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            var isUri = Uri.IsWellFormedUriString(e.Url.ToString(), UriKind.RelativeOrAbsolute);
            if(isUri && e.Url.ToString().StartsWith("http://"))
            {
                e.Cancel = true;
                try
                {
                    Process.Start(e.Url.ToString());
                    return;
                }
                catch (Exception) {}

                try
                {
                    Process.Start("firefox.exe", e.Url.ToString());
                    return;
                }
                catch (Exception) { }

                try
                {
                    Process.Start("chrome.exe", e.Url.ToString());
                    return;
                }
                catch (Exception) { }
            }
        }

        private void ErrorWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseProgram();
        }
    }
}
