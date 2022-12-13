using System.Text;

namespace AiboteSharp;
public static class Helper
{
    public static byte[] CombineWithParams(string functionName, params object[] ps)
    {
        StringBuilder sb = new(512);
        sb.Append(functionName.Length);
        sb.Append('/');
        sb.AppendJoin('/', ps.Select(x => Encoding.UTF8.GetBytes(x.ToString()).Length).ToArray());
        sb.Append('\n');
        sb.Append(functionName);
        sb.Append(string.Concat(ps.Select(x => x.ToString())));
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
    public static Point[]? getPoints(string r)
    {
        if (r.Contains('-') || r.Length == 0)
        {
            return null;
        }
        string[] points = r.Split('/');
        Point[] p = points.Select(x =>
        {
            string[] s = x.Split('|').Select(y => y[..(y.Length - 2)]).ToArray();
            return new Point { x = Convert.ToInt32(s[0]), y = Convert.ToInt32(s[1]) };
        }).ToArray();
        if (p.Length > 0)
        {
            return p;
        }
        return null;
    }
    public static Point? getPoint(string r)
    {
        if (r.Contains('-') || r.Length == 0)
        {
            return null;
        }
        string[] s = r.Split('|').Select(x => x[..(x.Length - 2)]).ToArray();
        return new Point { x = Convert.ToInt32(s[0]), y = Convert.ToInt32(s[1]) };
    }
    public static bool GetBool(string r) => r.Contains('t');
}

