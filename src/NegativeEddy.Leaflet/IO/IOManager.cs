using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace NegativeEddy.Leaflet.IO
{
    public class InterpreterOutput : IZOutput
    {
        public InterpreterOutput()
        {
        }

        private Subject<string> _outputSubject = new Subject<string>();
        private Subject<ScreenOpEventArgs> _screenOpsSubject = new Subject<ScreenOpEventArgs>();
        
        public IObservable<string> Print { get { return _outputSubject.AsObservable(); } }
        public IObservable<ScreenOpEventArgs> ScreenOps { get { return _screenOpsSubject.AsObservable(); } }

        public void WriteOutputLine(string text = null)
        {
            if (text != null)
            {
                WriteOutput(text);
            }
            WriteOutput(Environment.NewLine);
        }

        public void WriteOutput(string text)
        {
            _outputSubject.OnNext(text);
        }

        public void WriteOutputLine(object text)
        {
            if (text != null)
            {
                WriteOutput(text);
            }
            WriteOutput(Environment.NewLine);
        }

        public void WriteOutput(object text)
        {
            _outputSubject.OnNext(text.ToString());
        }

        public void ClearScreen(int window = -1)
        {
            _screenOpsSubject.OnNext(new ScreenOpEventArgs
            {
                Operation = window == -1 ? ScreenOperation.ClearScreen : ScreenOperation.EraseWindow,
                Window = window
            });
        }

        public void SplitWindow(int lines)
        {
            _screenOpsSubject.OnNext(new ScreenOpEventArgs
            {
                Operation = ScreenOperation.SplitWindow,
                Lines = lines
            });
        }

        public void SetWindow(int window)
        {
            _screenOpsSubject.OnNext(new ScreenOpEventArgs
            {
                Operation = ScreenOperation.SetWindow,
                Window = window
            });
        }

        public void SetCursor(int row, int column)
        {
            _screenOpsSubject.OnNext(new ScreenOpEventArgs
            {
                Operation = ScreenOperation.SetCursor,
                Row = row,
                Column = column
            });
        }

        public void SetTextStyle(int style)
        {
            _screenOpsSubject.OnNext(new ScreenOpEventArgs
            {
                Operation = ScreenOperation.SetTextStyle,
                TextStyle = style
            });
        }
    }
}
