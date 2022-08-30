using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot.Overlays
{
    public interface IProgrammingOverlay : IDisposable
    {
        public void Initialize(string blocklyPath);
        public bool Showing { get; }
        public void Show();
        public void Hide();
        public void Update();

    }
}
