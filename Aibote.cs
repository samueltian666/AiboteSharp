using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TouchSocket.Sockets;
using static AiboteSharp.Helper;
namespace AiboteSharp;

public class Aibote
{
    public readonly string _aid;
    public SocketClient _client;
    public ConcurrentQueue<MyMisson> _MissonQueue = new();
    private CancellationTokenSource cancelTokenSource = new();
    private bool combineFile = false;
    private List<byte> tempByte = new();
    private int failedt = 0;
    private DateTime lastsendtime = DateTime.Now;
    public Aibote(string aid, SocketClient s)
    {
        _client = s;
        _aid = aid;
    }
    public virtual void AddMisson(string fn, params object[] ps) => _MissonQueue.Enqueue(new MyMisson() { command = fn, data = ps });
    protected virtual void DealWithException(Exception e)
    {
        if (e.GetType() == typeof(NotConnectedException))
        {
            Console.WriteLine($"NotConnectedException   {e.Message}");
        }
        else if (e.Message.Contains("timed out"))
        {
            Console.WriteLine($"{DateTime.Now}timed out   {e.Message}");
            failedt += 1;
            if (failedt > 2)
            {
                Console.WriteLine("failedt > 2 ForceDestroyTask");
                ForceDestroyTask();
            }
        }
        else
        {
            Console.WriteLine(e.Message);
        }
    }
    protected virtual async void DealWithMission(MyMisson m)
    {
        if (m.command == "pushFile")
        {
            bool r = await pushFile(m.data[0].ToString(), m.data[1].ToString());
            Console.WriteLine($"pushfile {r}");
        }
        else if (m.command == "pullFile")
        {
            bool r = await pullFile(m.data[0].ToString(), m.data[1].ToString());
            Console.WriteLine($"pullFile {r}");
        }
        else
        {
            string r = await SendData(CombineWithParams(m.command, m.data));
            Console.WriteLine(r);
        }
    }
    public virtual async void Start()
    {
        while (!cancelTokenSource.IsCancellationRequested)
        {
            if (_MissonQueue.IsEmpty)
            {
                if ((DateTime.Now - lastsendtime).TotalSeconds > 28)
                {
                    await SendData(CombineWithParams("getAndroidId"));
                }
            }
            _MissonQueue.TryDequeue(out var m);
            if (m != null)
            {
                DealWithMission(m);
            }
            await Task.Delay(100);
        }
        throw new TaskCanceledException();
    }
    public virtual void MissonClear()=> _MissonQueue.Clear();
    public virtual void ForceDestroyTask()
    {
        cancelTokenSource.Cancel();
        _client.Dispose();
    }
    public void CombineByte(byte[] m)
    {
        if (combineFile)
            tempByte.AddRange(m);
    }
    private async Task<byte[]> sendDataReturnBytes(byte[] message)
    {
        try
        {
            combineFile = true;
            byte[] returnData =await _client.GetWaitingClient(new WaitingOptions() {ThrowBreakException=false,BreakTrigger=true }).SendThenReturnAsync(message);
            int index = 1;
            for (int i = 1; i < returnData.Length; i++)
            {
                if (returnData[i] == 47)
                {
                    index += i;
                    break;
                }
                if (i > 4)
                    break;
            }
            CombineByte(returnData.Skip(index).ToArray());
            lastsendtime = DateTime.Now;
            failedt = 0;
            await Task.Delay(5000);
            return tempByte.ToArray();
        }
        catch (Exception e)
        {
            DealWithException(e);
        }
        return Array.Empty<byte>();
    }
    /// <parm>保存的图片路径（手机）"/storage/emulated/0/Android/data/com.aibot.client/files/1.png</parm>
    /// <parm1>左上xy 右下xy 二值化算法类型 阈值 最大值 80, 150, 30, 30, 0, 127, 255 </parm1>
    /// <summary>
    /// 获取安卓ID
    /// </summary>
    public async Task<string> getAndroidId() => await SendData(CombineWithParams("getAndroidId"));
    public async Task<bool> saveScreenshot(params object[] ps) => GetBool(await SendData(CombineWithParams("saveScreenshot", ps)));
    /// <parm> x y坐标100 200</parm>
    /// <returns>#000000</returns>
    public async Task<string> getColor(params object[] ps) => await SendData(CombineWithParams("getColor", ps));
    /// <parm>保存的图片路径（手机）"/storage/emulated/0/Android/data/com.aibot.client/files/1.png</parm>
    /// <parm1>左上xy 右下xy 相似度 二值化算法类型 阈值 最大值 0, 0, 0, 0, 0.95, 0, 0, 0 </parm1>
    public async Task<Point?> findImage(params object[] ps) => getPoint(await SendData(CombineWithParams("findImage", ps)));
    /// <parm>保存的图片路径（手机）"/storage/emulated/0/Android/data/com.aibot.client/files/1.png</parm>
    /// <parm1>左上xy 右下xy 相似度  阈值 最大值 多个坐标点 0, 0, 0, 0, 0.95, 0, 0, 1 </parm1>
    public async Task<Point[]?> matchTemplate(params object[] ps) => getPoints(await SendData(CombineWithParams("matchTemplate", ps)));
    /// <parm>主颜色值, 相对偏移的颜色点 以 "x坐标+y坐标+色值" 字符串形式 , 左上xy 右下xy ,相似度</parm>
    /// <parm1>"#e8f2f8", "1020#e7f0f7", 0, 0, 0, 0, 1 </parm1>
    public async Task<Point?> findColor(params object[] ps) => getPoint(await SendData(CombineWithParams("findColor", ps)));
    /// <parm>主颜色值所在的XY, 主颜色值 ,相对偏移的颜色点以 "x坐标+y坐标+色值" 字符串形式 ,左上xy,右下xy ,相似值</parm>
    /// <parm1>100, 200, "#e8f2f8", "1020#e7f0f7", 0, 0, 0, 0, 1</parm1>
    public async Task<bool> compareColor(params object[] ps) => GetBool(await SendData(CombineWithParams("compareColor", ps)));
    /// <parm>xy,按下持续时间 ms</parm>
    public async Task<bool> press(params int[] ps) => GetBool(await SendData(CombineWithParams("press", ps)));
    /// <parm>xy,按下持续时间 ms</parm>
    public async Task<bool> move(params int[] ps) => GetBool(await SendData(CombineWithParams("move", ps)));
    public async Task<bool> release() => GetBool(await SendData(CombineWithParams("release")));
    /// <parm>xy</parm>
    public async Task<bool> click(params object[] ps) => GetBool(await SendData(CombineWithParams("click", ps)));

    /// <parm>xy</parm>
    public async Task<bool> doubleClick(params object[] ps) => GetBool(await SendData(CombineWithParams("doubleClick", ps)));
    /// <parm>xy 按下持续时间 ms</parm>
    public async Task<bool> longClick(params object[] ps) => GetBool(await SendData(CombineWithParams("longClick", ps)));
    /// <parm>开始xy,结束xy,滑动时间</parm>
    /// <parm1> 10, 10, 200, 200, 1000</parm1>
    public async Task<bool> swipe(params object[] ps) => GetBool(await SendData(CombineWithParams("swipe", ps)));
    /// <parm>执行手势坐标位 以"/"分割横纵坐标 "\n"分割坐标点。注意：末尾坐标点没有\n结束,手势耗时</parm>
    /// <parm1>3000, ponin[]</parm1>
    public async Task<bool> dispatchGesture(int duration, params Point[] points)
    {
        StringBuilder sb = new ();
        foreach(var v in points)
        {
            sb.Append(v.x);
            sb.Append('/');
            sb.Append(v.y);
            sb.Append('\n');
        }
        sb.Remove(sb.Length-1,1);
        return GetBool(await SendData(CombineWithParams("dispatchGesture", sb.ToString(),duration)));
    }
    /// <parm>执行多个手势坐标位， 以"/"分割手势时长、横纵和坐标 "\n"分割坐标点。"\r\n"分割多个手势</parm>
    /// <parm1> 1000/100/100\n200/200\r\n2000/300/300\n500/500</parm1>
    public async Task<bool> dispatchGestures(Point[][] points,params int[] durations)
    {
        if (points.GetLength(0) != durations.Length) return false;
        StringBuilder sb = new();
        int index = 0;
        foreach(var v in points)
        {
            foreach(var vv in v)
            {
                sb.Append(durations[index]);
                sb.Append(vv.x);
                sb.Append('/');
                sb.Append(vv.y);
                sb.Append('\n');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("\r\n");
            index += 1;
        }
        sb.Remove(sb.Length - 2, 2);
        return GetBool(await SendData(CombineWithParams("dispatchGestures", sb.ToString())));
    }
    /// <parm>文本内容</parm>
    /// <parm1>"aibote"</parm1>
    public async Task<bool> sendKeys(string ps) => GetBool(await SendData(CombineWithParams("sendKeys", ps)));
    public async Task<bool> back() => GetBool(await SendData(CombineWithParams("back")));
    public async Task<bool> home() => GetBool(await SendData(CombineWithParams("home")));
    public async Task<bool> recents() => GetBool(await SendData(CombineWithParams("recents")));
    /// <parm>虚拟键值 按键对照表 https://blog.csdn.net/yaoyaozaiye/article/details/122826340</parm>
    /// <parm1>187</parm1>
    public async Task<bool> sendVk(int ps) => GetBool(await SendData(CombineWithParams("sendVk", ps)));
    /// <parm>启动APP的名称或者包名</parm>
    /// <parm1>"QQ"</parm1>
    public async Task<bool> startApp(string ps) => GetBool(await SendData(CombineWithParams("startApp", ps)));
    /// <summary>
    /// 获取屏幕大小
    /// </summary>
    /// <returns>x=width y = height</returns>
    public async Task<Point> getWindowSize() => getPoint(await SendData(CombineWithParams("getWindowSize")));
    /// <summary>
    /// 获取图片大小
    /// </summary>
    /// <param name="ps"> "/storage/emulated/0/Android/data/com.aibot.client/files/1.png"</param>
    /// <returns>x=width y = height</returns>
    public async Task<Point?> getImageSize(string ps) => getPoint(await SendData(CombineWithParams("getImageSize", ps)));
    /// <summary>
    /// ocr
    /// </summary>
    public async Task<OCRResult[]?> ocr(params object[] ps)
    {
        string r = await SendData(CombineWithParams("ocr", ps));
        if (r.Length == 0)
        {
            return null;
        }
        Regex regex = new("[\\[\\]\\'\\(\\)]");
        //string s = @"[[[7.0, 18.0], [61.0, 18.0], [61.0, 38.0], [7.0, 38.0]], ('办公自动化', 0.8806074261665344)][[[4.0, 94.0], [49.0, 94.0], [49.0, 118.0], [4.0, 118.0]], ('rpa', 0.978314220905304)]";
        var result = r.Split("][")
            .Select(x => x.Split("],")
                .Select(y => regex.Replace(y, ""))
                .Select(z => z.Split(','))).Select(xx =>
                {
                    var xxx = xx.ToArray();
                    return new OCRResult()
                    {
                        lt = new Point() { x = (int)Convert.ToDouble(xxx[0][0]), y = (int)Convert.ToDouble(xxx[0][1]) },
                        rt = new Point() { x = (int)Convert.ToDouble(xxx[1][0]), y = (int)Convert.ToDouble(xxx[1][1]) },
                        ld = new Point() { x = (int)Convert.ToDouble(xxx[2][0]), y = (int)Convert.ToDouble(xxx[2][1]) },
                        rd = new Point() { x = (int)Convert.ToDouble(xxx[3][0]), y = (int)Convert.ToDouble(xxx[3][1]) },
                        word = xxx[4][0],
                        rate = Convert.ToDouble(xxx[4][1])
                    };
                })
            .ToArray();
        return result;
    }
    ///<parm>"网址", "GET", "null", "null" </parm>
    /// <returns>返回请求数据内容</returns>
    public async Task<string> urlRequest(params string[] ps) => await SendData(CombineWithParams("urlRequest", ps));
    /// <summary>
    /// Toast消息提示
    /// </summary>
    public async Task<bool> showToast(string ps) => GetBool(await SendData(CombineWithParams("showToast", ps)));
    /// <summary>
    /// 识别验证码
    /// </summary>
    /// <param name="ps">"/storage/emulated/0/Android/data/com.aibot.client/files/1.png", "username", "password", "123456", "1004", "0"</param>
    /// <returns>{{"err_no":0,"err_str":"OK","pic_id":"9160109360600112681","pic_str":"8vka","md5":"35d5c7f6f53223fbdc5b72783db0c2c0"}</returns>>
    public async Task<Capture?> getCaptcha(params string[] ps)
    {
        string r = await SendData(CombineWithParams("getCaptcha", ps));
        if (r != String.Empty)
        {
            return JsonSerializer.Deserialize<Capture>(r);
        }
        return null;
    }
    /// <summary>
    /// 识别报错返分
    /// </summary>
    /// <param name="ps">"errorCaptcha", "username", "password", "123456", "9160109360600112681"</param>
    /// <returns>{"err_no":0,"err_str":"OK"}</returns>>
    public async Task<Capture?> errorCaptcha(params string[] ps)
    {
        string r = await SendData(CombineWithParams("errorCaptcha", ps));
        if (r != String.Empty)
        {
            return JsonSerializer.Deserialize<Capture>(r);
        }
        return null;
    }
    /// <summary>
    /// 查询验证码剩余题分
    /// </summary>
    /// <param name="ps">"scoreCaptcha", "username", "password"</param>
    /// <returns>{"err_no":0,"err_str":"OK","tifen":821690,"tifen_lock":0}</returns>>
    public async Task<Capture?> scoreCaptcha(params string[] ps)
    {
        string r = await SendData(CombineWithParams("scoreCaptcha", ps));
        if (r != String.Empty)
        {
            return JsonSerializer.Deserialize<Capture>(r);
        }
        return null;
    }
    /// <summary>
    /// 上传文件
    /// </summary>
    public async Task<bool> pushFile(string phonePath, string fileName)
    {
        string filepath = $@"./files/{fileName}";
        if (!File.Exists(filepath))
        {
            return false;
        }
        byte[] data = File.ReadAllBytes(filepath);
        List<byte> bytes = new();
        StringBuilder sb = new();
        sb.Append(Encoding.UTF8.GetBytes("pushFile").Length);
        sb.Append('/');
        sb.Append(Encoding.UTF8.GetBytes(phonePath).Length);
        sb.Append('/');
        sb.Append(data.Length);
        sb.Append('\n');
        sb.Append("pushFile");
        sb.Append(phonePath);
        bytes.AddRange(Encoding.UTF8.GetBytes(sb.ToString()));
        bytes.AddRange(data);
        return GetBool(await SendData(bytes.ToArray()));
    }
    /// <summary>
    /// 拉取文件
    /// </summary>
    public async Task<bool> pullFile(string phonePath, string savename)
    {
        string savePath = $@"./files/{savename}";
        byte[] data = await sendDataReturnBytes(CombineWithParams("pullFile", phonePath));
        if (data.Length == 0 || data.Length == 4)
        {
            return false;
        }
        FileStream fs = new(savePath, FileMode.Create);
        fs.Write(data, 0, data.Length);
        fs.Dispose();
        tempByte.Clear();
        return File.Exists(savePath);
    }
    /// <summary>
    /// 写入安卓文件
    /// </summary>
    public async Task<bool> writeAndroidFile(string phonePath, string content, bool b = false) => GetBool(await SendData(CombineWithParams("writeAndroidFile", phonePath, content, b)));
    /// <summary>
    /// 读取安卓文件
    /// </summary>
    public async Task<string> readAndroidFile(string phonePath) => await SendData(CombineWithParams("readAndroidFile", phonePath));
    /// <summary>
    /// 跳转uri
    /// </summary>
    public async Task<bool> openUri(string ps) => GetBool(await SendData(CombineWithParams("opemUri", ps)));

    /// <summary>
    /// 拨打电话
    /// </summary>
    public async Task<bool> callPhone(string ps) => GetBool(await SendData(CombineWithParams("callPhone", ps)));

    /// <summary>
    /// 发送短信
    /// </summary>
    public async Task<bool> sendMsg(params string[] ps) => GetBool(await SendData(CombineWithParams("sendMsg", ps)));
    /// <summary>
    /// 获取活动窗口
    /// </summary>
    /// <returns>.MainActivity</returns>>
    public async Task<string> activity() => await SendData(CombineWithParams("sendMsg"));

    /// <summary>
    /// 获取活动包名
    /// </summary>
    /// <returns>com.aibot.client</returns>>
    public async Task<string> getPackage() => await SendData(CombineWithParams("getPackage"));
    /// <summary>
    /// 设置剪切板
    /// </summary>
    public async Task<bool> setClipboardText(string ps) => GetBool(await SendData(CombineWithParams("setClipboardText", ps)));
    /// <summary>
    /// 获取剪切板
    /// </summary>
    public async Task<string> getClipboardText() => await SendData(CombineWithParams("getClipboardText"));
    /// <summary>
    /// 获取元素位置
    /// </summary>
    /// <returns>[1,2,3,4]</returns>>
    public async Task<int[]> getElementRect(string ps)
    {
        string r = await SendData(CombineWithParams("getElementRect", ps));
        return r.Split('|').Cast<int>().ToArray();
    }
    /// <summary>
    /// 获取元素描述
    /// </summary>
    /// <returns>192.168.2.7</returns>
    public async Task<string> getElementDescription(string ps) => await SendData(CombineWithParams("getElementDescription", ps));
    /// <summary>
    /// 获取元素文本
    /// </summary>
    /// <returns>192.168.2.7</returns>>
    public async Task<string> getElementText(string ps) => await SendData(CombineWithParams("getElementText", ps));
    /// <summary>
    /// 点击元素
    /// </summary>
    public async Task<bool> clickElement(string ps) => GetBool(await SendData(CombineWithParams("clickElement", ps)));
    /// <summary>
    /// 设置元素文本
    /// </summary>
    public async Task<bool> setElementText(params string[] ps) => GetBool(await SendData(CombineWithParams("setElementText", ps)));
    /// <summary>
    /// 滚动元素
    /// </summary>
    public async Task<bool> scrollElement(params object[] ps) => GetBool(await SendData(CombineWithParams("scrollElement", ps)));
    /// <summary>
    /// 判断元素是否存在
    /// </summary>
    public async Task<bool> existsElement(string ps) => GetBool(await SendData(CombineWithParams("existsElement", ps)));
    /// <summary>
    /// 判断元素是否选中
    /// </summary>
    public async Task<bool> isSelectedElement(string ps) => GetBool(await SendData(CombineWithParams("isSelectedElement", ps)));

    /// <summary>
    /// 创建TextView控件
    /// </summary>
    /// <parm1>100, "Aibote TextView", 10, 10, 300, 100</parm1>
    public async Task<bool> createTextView(params object[] ps) => GetBool(await SendData(CombineWithParams("createTextView", ps)));
    /// <summary>
    /// 创建EditText
    /// </summary>
    /// <parm1>101, "Aibote EditText", 10, 10, 300, 100</parm1>
    public async Task<bool> createEditText(params object[] ps) => GetBool(await SendData(CombineWithParams("createEditText", ps)));

    /// <summary>
    /// 创建CheckBox控件
    /// </summary>
    /// <parm1>102, "Aibote CheckBox", 10, 10, 300, 100</parm1>
    public async Task<bool> createCheckBox(params object[] ps) => GetBool(await SendData(CombineWithParams("createCheckBox", ps)));
    /// <summary>
    /// 创建WebView控件
    /// </summary>
    /// <parm1>103, "http://www.aibote.net", -1, -1, -1, -1</parm1>
    public async Task<bool> createWebView(params object[] ps) => GetBool(await SendData(CombineWithParams("createWebView", ps)));
    /// <summary>
    /// 清除脚本控件
    /// </summary>
    public async Task<bool> clearScriptControl(params object[] ps) => GetBool(await SendData(CombineWithParams("clearScriptControl", ps)));
    /// <summary>
    /// 获取脚本配置参数
    /// </summary>
    /// <returns>{"100":"Aibote TextView"}</returns>
    public async Task<string> getScriptParam() =>await SendData(CombineWithParams("getScriptParam"));
    private async Task<string> SendData(byte[] message)
    {
        try
        {
            byte[] returnData = await _client.GetWaitingClient(new WaitingOptions() { ThrowBreakException = false, BreakTrigger = true }).SendThenReturnAsync(message);
            int index = 1;
            for (int i = 1; i < returnData.Length; i++)
            {
                if (returnData[i] == 47)
                {
                    index += i;
                    break;
                }
                if (i > 4)
                    break;
            }
            byte[] n = returnData.Skip(index).ToArray();
            lastsendtime = DateTime.Now;
            failedt = 0;
            combineFile = false;
            return Encoding.UTF8.GetString(n); ;
        }
        catch (Exception e)
        {
            DealWithException(e);
        }
        return string.Empty;
    }
}