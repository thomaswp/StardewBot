using CefSharp.OffScreen;
using System;

namespace Browser.Host
{
    class Program
    {

        static void Main(string[] args)
        {
            var host = new BrowserHost(new Interchange());
            while (!host.IsDisposed) ;
        }


    }
}
