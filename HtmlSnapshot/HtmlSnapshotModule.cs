using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace HtmlSnapshot
{
    public class HtmlSnapshotModule : IHttpModule
    {

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public string PhantomJsPath = string.Empty;
        public string ExecPath = string.Empty;

        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
            this.ExecPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            this.PhantomJsPath = Path.Combine(ExecPath, "phantomjs.exe");
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            var context = (sender as HttpApplication);
            if (context.Context.Request.Browser.Crawler
                && string.IsNullOrEmpty(context.Context.Request.CurrentExecutionFilePathExtension)
                && context.Context.Request.Url.PathAndQuery.Split(new char[] { '/' }).Intersect(new string[] { "bundles", "scripts", "styles" }).Count() == 0)
            {
                //Trace prerendered Url
                Trace.WriteLine(context.Request.Url);
                //do predrender
                var c = new Command(PhantomJsPath, "--load-images=false prerender.js " + context.Request.Url, this.ExecPath);

                c.Run(5000);

                if (c.HasExited && c.ExitCode == 0)
                {
                    context.Response.Headers.Add("Content-Type", "text/html; charset=utf-8");
                    context.Response.Write("Powered By PhantomJS");
                    context.Response.Write(c.StandardOutput);
                   
                    context.Response.End();
                }

            }

        }
    }
}