namespace AiboteSharp;

public sealed class MyMisson
{
    public string command { get; set; }
    public object[] data { get; set; } = new object[] { };
}
public sealed class Point
{
    public int x;
    public int y;
}
public sealed class OCRResult
{
    public Point lt;
    public Point rt;
    public Point ld;
    public Point rd;
    public string word;
    public double rate;
}
public sealed class Capture
{
    public int err_no { get; set; }
    public string err_str { get; set; }
    public string? pic_id { get; set; }
    public string? pic_str { get; set; }
    public string? md5 { get; set; }
    public int? tifen { get; set; }
    public int? tifen_lock { get; set; }
}
