namespace SqlFormatter
{
    public class SQLFormatterResponse
    {
        public string Text { get; set; }
        public ErrorInfo ErrorInfo { get; set; }
        public CaretPosition CaretPosition { get; set; }
        public bool InDemoMode { get; set; }
    }

    public class ErrorInfo
    {
        public string ErrorMessage { get; set; }
        public ErrorSpan ErrorSpan { get; set; }
    }

    public class ErrorSpan
    {
        public int Bottom { get; set; }
        public bool IsEmpty { get; set; }
        public End End { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public Start Start { get; set; }
        public Size Size { get; set; }
        public int Top { get; set; }
    }

    public class End
    {
        public bool IsEmpty { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Start
    {
        public bool IsEmpty { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Size
    {
        public bool IsEmpty { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class CaretPosition
    {
        public bool IsEmpty { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

}
