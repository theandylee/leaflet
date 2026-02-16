using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using NegativeEddy.Leaflet.IO;

namespace NegativeEddy.Leaflet.ConsoleHost
{
    class Program
    {
        private static int _currentWindow = 0;
        private static int _upperWindowHeight = 0;
        private static int _window0CursorRow = 0;
        private static int _window0CursorCol = 0;
        private static int _window1CursorRow = 0;
        private static int _window1CursorCol = 0;

        static void Main(string[] args)
        {
            var zm = new Interpreter();
            zm.Input = new ConsoleInput();
            zm.Output.Subscribe(OnOutput);
            zm.ScreenOps.Subscribe(OnScreenOp);
            zm.Diagnostics.Subscribe(x => Debug.Write(x));
            zm.DiagnosticsOutputLevel = Interpreter.DiagnosticsLevel.Off;

            string filename = Path.Combine("GameFiles", "TRINITY.DAT");
            using (var stream = File.OpenRead(filename))
            {
                zm.LoadStory(stream);
            }
            Console.WriteLine($"Gamefile Version {zm.MainMemory.Header.Version}");

            //DumpObjectTree(zm);

            //DumpObjects(zm);

            RunGame(zm);
        }

        private static void DumpObjectTree(Interpreter zm)
        {
            string output = zm.MainMemory.ObjectTree.DumpObjectTree();
            Console.WriteLine(output);
            Console.WriteLine($"Object tree contains {zm.MainMemory.ObjectTree.Objects.Count} objects");
        }

        private static void DumpObjects(Interpreter zm)
        {
            var objTree = zm.MainMemory.ObjectTree;

            foreach (var obj in objTree)
            {
                Console.WriteLine(obj.ToFullString());
            }
        }

        private static void RunGame(Interpreter zm)
        {
            while (zm.IsRunning)
            {
                zm.ExecuteCurrentInstruction();
            }
        }

        private static void OnOutput(string text)
        {
            if (_currentWindow == 1) return;
            
            Console.Write(text);
            if (_currentWindow == 0)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        _window0CursorRow++;
                        _window0CursorCol = 0;
                    }
                    else
                    {
                        _window0CursorCol++;
                    }
                }
            }
        }

        private static void OnScreenOp(ScreenOpEventArgs args)
        {
            switch (args.Operation)
            {
                case ScreenOperation.ClearScreen:
                    Console.Clear();
                    break;
                case ScreenOperation.EraseWindow:
                    if (args.Window == -1)
                    {
                        Console.Clear();
                    }
                    else if (args.Window == -2)
                    {
                        Console.Clear();
                    }
                    else
                    {
                        Console.Clear();
                    }
                    break;
                case ScreenOperation.SplitWindow:
                    _upperWindowHeight = args.Lines;
                    break;
                case ScreenOperation.SetWindow:
                    if (_currentWindow == 0)
                    {
                        _window0CursorRow = Console.CursorTop;
                        _window0CursorCol = Console.CursorLeft;
                    }
                    else if (_currentWindow == 1)
                    {
                        _window1CursorRow = Console.CursorTop;
                        _window1CursorCol = Console.CursorLeft;
                    }
                    _currentWindow = args.Window;
                    if (_currentWindow == 0)
                    {
                        Console.SetCursorPosition(_window0CursorCol, _window0CursorRow + _upperWindowHeight);
                    }
                    else if (_currentWindow == 1)
                    {
                        Console.SetCursorPosition(_window1CursorCol, _window1CursorRow);
                    }
                    break;
                case ScreenOperation.SetCursor:
                    if (_currentWindow == 1)
                    {
                        Console.SetCursorPosition(args.Column - 1, args.Row - 1);
                        _window1CursorRow = args.Row - 1;
                        _window1CursorCol = args.Column - 1;
                    }
                    break;
                case ScreenOperation.SetTextStyle:
                    int style = args.TextStyle;
                    if (style == 0)
                    {
                        Console.ResetColor();
                    }
                    else if ((style & 2) != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    break;
            }
        }
    }
}
