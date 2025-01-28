using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;

namespace BowThrusterChallenge.Settings
{
    public class Settings
    {
        public WindowSettings Window { get; set; } // Window settings (Width, Height, Title)
        public ColorSettings BackgroundColor { get; set; } // Background color (RGBA)
        public ControlSettings Controls { get; set; } // Control key mappings
    }

    // A class for the window settings (Width, Height, Title)
    public class WindowSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Title { get; set; }
    }

    // A class for the background color settings (RGBA values)
    public class ColorSettings
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }
    }

    // A class for the controls (key mappings)
    public class ControlSettings
    {
        public string Go { get; set; }
        public string RudderLeft { get; set; }
        public string RudderRight { get; set; }
        public string ThrusterLeft { get; set; }
        public string ThrusterRight { get; set; }
        public string Restart { get; set; }
        public string Menu { get; set; }
        public string Close { get; set;}
        public string Select { get; set;}
        public string MenuUp { get; set;}
        public string MenuDown { get; set;}
    } 
}
