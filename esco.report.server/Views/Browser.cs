using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace esco.report.server
{
    partial class Browser : Form
    {
        public Browser()
        {
            InitializeComponent();
            this.Resize += new System.EventHandler(this.Form_Resize);
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            webView.Size = this.ClientSize - new System.Drawing.Size(webView.Location);
        }

        public Browser(bool oncloud, string url, string user, string pass)
        {
            InitializeComponent();
            string domain = (user.Contains("\\")) ? user.Split('\\')[0] : null;
            user = (user.Contains("\\")) ? user.Split('\\')[1] : user;

            if (oncloud)
            {
                //On Cloud
                //First Authenticate
                _ = Navigate(url);
            }
            else
            {
                //On Premise
                string http = (url.Contains(Config.https)) ? Config.https : Config.http;
                string _url = http + user + ":" + pass + "@" + url.Replace(http, String.Empty);
                _ = Navigate(_url);
            }
        }

        public async Task Navigate(string url)
        {        
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Navigate(url);
        }
    }
}
