using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Browser.Common
{
    public static class BrowserSettings
    {

        public const string MEMORY_NAME = @"BrowserBuffer";
        public const int MAX_WIDTH = 1024, MAX_HEIGHT = 800;
        // TODO: Add header
        public const int NODE_SIZE = 4 * MAX_WIDTH * MAX_HEIGHT;


    }
}
