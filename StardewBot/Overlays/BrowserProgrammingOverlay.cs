using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsAPI;

namespace StardewBot.Overlays
{
    public class BrowserProgrammingOverlay : IProgrammingOverlay
    {
        private BrowserOverlay overlay;

        public bool Showing { get; private set; }

        private bool initialResize = false;

        public BrowserProgrammingOverlay()
        {
            overlay = new BrowserOverlay();
        }
        public void Initialize(string blocklyPath, string browserPath)
        {
            // TODO: Be more precise
            Task.Delay(1000).ContinueWith(t =>
            {
                overlay.Initialize(blocklyPath, browserPath, p => p.ProcessName == "StardewModdingAPI");
            });
        }
        
        // TODO: Need a resize method here too to reload chrome if it's resized bigger...
        // and just in gerenal in case of altf4 or other crashes...

        public void Dispose()
        {
            overlay.Dispose();
        }

        public void Hide()
        {
            overlay.Hide();
            Showing = false;
        }

        public void Show()
        {
            // TODO: Need to add a menu so stardew pauses...
            overlay.Show();
            Showing = true;
        }

        public void Update()
        { 
        }
    }
}
