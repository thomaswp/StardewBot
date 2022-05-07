using CefSharp.OffScreen;
using System;

namespace BrowserHost
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
