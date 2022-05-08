using CefSharp.OffScreen;
using System;

namespace Browser.Host
{
    class Program
    {

        static void Main(string[] args)
        {
            new BrowserHost(new Interchange());

            Console.ReadLine();
        }


    }
}
