using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TouchSocket.Sockets;
using static AiboteSharp.Helper;
namespace AiboteSharp;

public class Aibote
{
    const byte b = 47;
    public SocketClient _client;
    public string _aid;
    public ConcurrentQueue<MyMisson> _MissonQueue = new();
    DateTime lastsendtime = DateTime.Now;
    public Aibote(string aid, SocketClient s)
    {
        _client = s;
        _aid = aid;
    }
    public virtual void AddMisson(string fn,object[] ps)
    {
        _MissonQueue.Enqueue(new MyMisson() {command=fn,data=ps });
    }
    public virtual void StartScript()
    {
        while (true)
        {
            if (_MissonQueue.IsEmpty)
            {
                if ((DateTime.Now - lastsendtime).TotalSeconds > 29)
                {
                    _MissonQueue.Enqueue(new MyMisson() { command = "getAndroidId" });
                }
            }
            _MissonQueue.TryDequeue(out var m);
            if (m != null)
            {
                if (m.command != "getAndroidId")
                {
                    string r = SendData(CombineWithParams(m.command, m.data));
                    Console.WriteLine(r);
                }
                Thread.Sleep(100);
            }
        }
    }

    public virtual void Stop()
    {
        throw new TaskCanceledException();
    }
    /// <parm>保存的图片路径（手机）"/storage/emulated/0/Android/data/com.aibot.client/files/1.png</parm>
    /// <parm1>左上xy 右下xy 二值化算法类型 阈值 最大值 80, 150, 30, 30, 0, 127, 255 </parm1>
    /// <summary>
    /// 获取安卓ID
    /// </summary>
    public string getAndroidId() => SendData(CombineWithParams("getAndroidId"));
    public bool saveScreenshot(params object[] ps) => GetBool(SendData(CombineWithParams("saveScreenshot", ps)));
    /// <parm> x y坐标100 200</parm>
    /// <returns>#000000</returns>
    public string getColor(params object[] ps) => SendData(CombineWithParams("getColor", ps));
    /// <parm>保存的图片路径（手机）"/storage/emulated/0/Android/data/com.aibot.client/files/1.png</parm>
    /// <parm1>左上xy 右下xy 相似度 二值化算法类型 阈值 最大值 0, 0, 0, 0, 0.95, 0, 0, 0 </parm1>
    public Point? findImage(params object[] ps) => getPoint(SendData(CombineWithParams("findImage", ps)));
    /// <parm>保存的图片路径（手机）"/storage/emulated/0/Android/data/com.aibot.client/files/1.png</parm>
    /// <parm1>左上xy 右下xy 相似度  阈值 最大值 多个坐标点 0, 0, 0, 0, 0.95, 0, 0, 1 </parm1>
    public Point[]? matchTemplate(params object[] ps) => getPoints(SendData(CombineWithParams("matchTemplate", ps)));
    /// <parm>主颜色值, 相对偏移的颜色点 以 "x坐标+y坐标+色值" 字符串形式 , 左上xy 右下xy ,相似度</parm>
    /// <parm1>"#e8f2f8", "1020#e7f0f7", 0, 0, 0, 0, 1 </parm1>
    public Point? findColor(params object[] ps) => getPoint(SendData(CombineWithParams("findColor", ps)));
    /// <parm>主颜色值所在的XY, 主颜色值 ,相对偏移的颜色点以 "x坐标+y坐标+色值" 字符串形式 ,左上xy,右下xy ,相似值</parm>
    /// <parm1>100, 200, "#e8f2f8", "1020#e7f0f7", 0, 0, 0, 0, 1</parm1>
    public bool compareColor(params object[] ps) => GetBool(SendData(CombineWithParams("compareColor", ps)));
    /// <parm>xy,按下持续时间 ms</parm>
    public bool press(params int[] ps) => GetBool(SendData(CombineWithParams("press", ps)));
    /// <parm>xy,按下持续时间 ms</parm>
    public bool move(params int[] ps) => GetBool(SendData(CombineWithParams("move", ps)));
    public bool release() => GetBool(SendData(CombineWithParams("release")));
    /// <parm>xy</parm>
    public bool click(params object[] ps) => GetBool(SendData(CombineWithParams("click", ps)));

    /// <parm>xy</parm>
    public bool doubleClick(params object[] ps) => GetBool(SendData(CombineWithParams("doubleClick", ps)));
    /// <parm>xy 按下持续时间 ms</parm>
    public bool longClick(params object[] ps) => GetBool(SendData(CombineWithParams("longClick", ps)));
    /// <parm>开始xy,结束xy,滑动时间</parm>
    /// <parm1> 10, 10, 200, 200, 1000</parm1>
    public bool swipe(params object[] ps) => GetBool(SendData(CombineWithParams("swipe", ps)));
    /// <parm>执行手势坐标位 以"/"分割横纵坐标 "\n"分割坐标点。注意：末尾坐标点没有\n结束,手势耗时</parm>
    /// <parm1> "1000/1558\n100/100\n799/800\n234/89", 3000</parm1>
    public bool dispatchGesture(params object[] ps) => GetBool(SendData(CombineWithParams("dispatchGesture", ps)));
    /// <parm>文本内容</parm>
    /// <parm1>"aibote"</parm1>
    public bool sendKeys(string ps) => GetBool(SendData(CombineWithParams("sendKeys", ps)));
    public bool back() => GetBool(SendData(CombineWithParams("back")));
    public bool home() => GetBool(SendData(CombineWithParams("home")));
    public bool recents() => GetBool(SendData(CombineWithParams("recents")));
    /// <parm>虚拟键值 按键对照表 https://blog.csdn.net/yaoyaozaiye/article/details/122826340</parm>
    /// <parm1>187</parm1>
    public bool sendVk(int ps) => GetBool(SendData(CombineWithParams("sendVk", ps)));
    /// <parm>启动APP的名称或者包名</parm>
    /// <parm1>"QQ"</parm1>
    public bool startApp(string ps) => GetBool(SendData(CombineWithParams("startApp", ps)));
    /// <summary>
    /// 获取屏幕大小
    /// </summary>
    /// <returns>x=width y = height</returns>
    public Point? getWindowSize() => getPoint(SendData(CombineWithParams("getWindowSize")));
    /// <summary>
    /// 获取图片大小
    /// </summary>
    /// <param name="ps"> "/storage/emulated/0/Android/data/com.aibot.client/files/1.png"</param>
    /// <returns>x=width y = height</returns>
    public Point? getImageSize(string ps) => getPoint(SendData(CombineWithParams("getImageSize", ps)));
    /// <summary>
    /// ocr
    /// </summary>
    public OCRResult[]? ocr(params object[] ps)
    {
        string r = SendData(CombineWithParams("ocr", ps));
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
                        lt = new Point() { x = Convert.ToDouble(xxx[0][0]), y = Convert.ToDouble(xxx[0][1]) },
                        rt = new Point() { x = Convert.ToDouble(xxx[1][0]), y = Convert.ToDouble(xxx[1][1]) },
                        ld = new Point() { x = Convert.ToDouble(xxx[2][0]), y = Convert.ToDouble(xxx[2][1]) },
                        rd = new Point() { x = Convert.ToDouble(xxx[3][0]), y = Convert.ToDouble(xxx[3][1]) },
                        word = xxx[4][0],
                        rate = Convert.ToDouble(xxx[4][1])
                    };
                })
            .ToArray();
        return result;
    }
    ///<parm>"网址", "GET", "null", "null" </parm>
    /// <returns>返回请求数据内容</returns>
    public string urlRequest(params string[] ps) => SendData(CombineWithParams("urlRequest", ps));
    /// <summary>
    /// Toast消息提示
    /// </summary>
    public bool showToast(string ps) => GetBool(SendData(CombineWithParams("showToast", ps)));
    /// <summary>
    /// 识别验证码
    /// </summary>
    /// <param name="ps">"/storage/emulated/0/Android/data/com.aibot.client/files/1.png", "username", "password", "123456", "1004", "0"</param>
    /// <returns>{{"err_no":0,"err_str":"OK","pic_id":"9160109360600112681","pic_str":"8vka","md5":"35d5c7f6f53223fbdc5b72783db0c2c0"}</returns>>
    public Capture? getCaptcha(params string[] ps)
    {
        string r = SendData(CombineWithParams("getCaptcha", ps));
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
    public Capture? errorCaptcha(params string[] ps)
    {
        string r = SendData(CombineWithParams("errorCaptcha", ps));
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
    public Capture? scoreCaptcha(params string[] ps)
    {
        string r = SendData(CombineWithParams("scoreCaptcha", ps));
        if (r != String.Empty)
        {
            return JsonSerializer.Deserialize<Capture>(r);
        }
        return null;
    }
    /// <summary>
    /// 上传文件
    /// </summary>
    public bool pushFile(string path, string file)
    {
        if (!File.Exists(file))
        {
            return false;
        }
        byte[] data = File.ReadAllBytes(file);
        return GetBool(SendData(CombineWithParams("pushFile", path, Encoding.UTF8.GetString(data))));
    }
    /// <summary>
    /// 拉取文件
    /// </summary>
    public bool pullFile(string phonePath, string savePath)
    {
        string r = SendData(CombineWithParams("pullFile", phonePath));
        if (r.Length == 0)
        {
            return false;
        }
        byte[] data = Encoding.UTF8.GetBytes(r);
        FileStream fs = new FileStream(savePath, FileMode.Create);
        fs.Write(data, 0, data.Length);
        fs.Dispose();
        return File.Exists(savePath);
    }
    /// <summary>
    /// 跳转uri
    /// </summary>
    public bool openUri(string ps) => GetBool(SendData(CombineWithParams("opemUri", ps)));

    /// <summary>
    /// 拨打电话
    /// </summary>
    public bool callPhone(string ps) => GetBool(SendData(CombineWithParams("callPhone", ps)));

    /// <summary>
    /// 发送短信
    /// </summary>
    public bool sendMsg(params string[] ps) => GetBool(SendData(CombineWithParams("sendMsg", ps)));
    /// <summary>
    /// 获取活动窗口
    /// </summary>
    /// <returns>.MainActivity</returns>>
    public string activity() => SendData(CombineWithParams("sendMsg"));

    /// <summary>
    /// 获取活动包名
    /// </summary>
    /// <returns>com.aibot.client</returns>>
    public string getPackage() => SendData(CombineWithParams("getPackage"));
    /// <summary>
    /// 设置剪切板
    /// </summary>
    public bool setClipboardText(string ps) => GetBool(SendData(CombineWithParams("setClipboardText", ps)));
    /// <summary>
    /// 获取剪切板
    /// </summary>
    public string getClipboardText() => SendData(CombineWithParams("getClipboardText"));
    /// <summary>
    /// 获取元素位置
    /// </summary>
    /// <returns>[1,2,3,4]</returns>>
    public int[] getElementRect(string ps)
    {
        string r = SendData(CombineWithParams("getElementRect", ps));
        return r.Split('|').Cast<int>().ToArray();
    }
    /// <summary>
    /// 获取元素描述
    /// </summary>
    /// <returns>192.168.2.7</returns>
    public string getElementDescription(string ps) => SendData(CombineWithParams("getElementDescription", ps));
    /// <summary>
    /// 获取元素文本
    /// </summary>
    /// <returns>192.168.2.7</returns>>
    public string getElementText(string ps) => SendData(CombineWithParams("getElementText", ps));
    /// <summary>
    /// 点击元素
    /// </summary>
    public bool clickElement(string ps) => GetBool(SendData(CombineWithParams("clickElement", ps)));
    /// <summary>
    /// 设置元素文本
    /// </summary>
    public bool setElementText(params string[] ps) => GetBool(SendData(CombineWithParams("setElementText", ps)));
    /// <summary>
    /// 滚动元素
    /// </summary>
    public bool scrollElement(params object[] ps) => GetBool(SendData(CombineWithParams("scrollElement", ps)));
    /// <summary>
    /// 判断元素是否存在
    /// </summary>
    public bool existsElement(string ps) => GetBool(SendData(CombineWithParams("existsElement", ps)));
    /// <summary>
    /// 判断元素是否选中
    /// </summary>
    public bool isSelectedElement(string ps) => GetBool(SendData(CombineWithParams("isSelectedElement", ps)));

    /// <summary>
    /// 创建TextView控件
    /// </summary>
    /// <parm1>100, "Aibote TextView", 10, 10, 300, 100</parm1>
    public bool createTextView(params object[] ps) => GetBool(SendData(CombineWithParams("createTextView", ps)));
    /// <summary>
    /// 创建EditText
    /// </summary>
    /// <parm1>101, "Aibote EditText", 10, 10, 300, 100</parm1>
    public bool createEditText(params object[] ps) => GetBool(SendData(CombineWithParams("createEditText", ps)));

    /// <summary>
    /// 创建CheckBox控件
    /// </summary>
    /// <parm1>102, "Aibote CheckBox", 10, 10, 300, 100</parm1>
    public bool createCheckBox(params object[] ps) => GetBool(SendData(CombineWithParams("createCheckBox", ps)));
    string SendData(byte[] message)
    {
        try
        {
            byte[] returnData = _client.GetWaitingClient().SendThenReturn(message);
            int index = 1;
            for (int i = 0; i < returnData.Length; i++)
            {
                if (returnData[i] == b)
                {
                    index += i;
                    break;
                }
            }
            byte[] n = returnData.Skip(index).ToArray();
            lastsendtime = DateTime.Now;
            return Encoding.UTF8.GetString(n); ;
        }
        catch (Exception e)
        {
            if (e.GetType() == typeof(NotConnectedException))
            {
                Console.WriteLine($"{DateTime.Now}--{_aid} NotConnectedException {e.Message}");
            }
            else if (e.Message.Contains("timed out"))
            {
                Console.WriteLine($"{DateTime.Now}--{_aid}timed out {e.Message}");
            }
            else
            {
                Console.WriteLine(e.Message);
            }
        }
        return string.Empty;
    }
}
