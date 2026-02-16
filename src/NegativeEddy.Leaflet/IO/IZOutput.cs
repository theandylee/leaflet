using System;

namespace NegativeEddy.Leaflet.IO
{
    public enum ScreenOperation
    {
        ClearScreen,
        EraseWindow,
        SplitWindow,
        SetWindow,
        SetCursor,
        SetTextStyle
    }

    public class ScreenOpEventArgs
    {
        public ScreenOperation Operation { get; set; }
        public int Window { get; set; }
        public int Lines { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int TextStyle { get; set; }
    }

    public interface IZOutput
    {
        IObservable<string> Print { get; }
        IObservable<ScreenOpEventArgs> ScreenOps { get; }
    }
}
