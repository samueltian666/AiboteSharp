﻿###########环境依赖  
dotnet6+touchsocket  
###########touchsocket文档  
https://www.yuque.com/rrqm/touchsocket/55e5bbf58745fa639dba511c7bcd54d1  
###########aibote协议文档  
http://www.aibote.net/aiboteProtocol.html  
###########nuget  
https://www.nuget.org/packages/AiboteSharp/  
###########提示  
开放接口可调用不限于AndoridBot的接口  
返回结果这个地方需要自行处理  
!!!由于web和window没有getAndroidId  
!!!所以需要重写Tcp下的connected和receive  
优先考虑本框架为server用websocket 或signalr桥接client配合使用~~~  
有需要成品或者定制可以+q详聊2716015135  
欢迎贡献提bug完善框架    
https://github.com/samueltian666/AiboteSharp  
###########使用  
```c#
Tcp tcp = new("0.0.0.0:5211");  
while (true)  
{  
    string v = Console.ReadLine();  
    if (v == "1")  
    {  
        //tcp.ais[0].AddMisson("startApp","com.android.settings");  
        //tcp.ais[0].AddMisson("home")  
        //注意Addmisson返回目前需要自行处理
        var b = tcp.ais[0].home();  
        Console.WriteLine(b);
    }  
}  
```
###########免责声明  
本工具用于个人学习使用，企业自查，严禁使用本工具对互联网造成破坏，感谢。  
###########v1.0.5  
更换了异步,dispatchGesture相关更新  
###########v1.0.4  
明确访问权限  
修复Aibote.MissonClear内socket client被关闭的错误  
拆分Aibote.sendDataReturn sendDataReturnBytes 中的错误处理 DealWithException  
拆分Aibote.Start内处理任务 DealWithMission  
拆分Tcp中received的新建task   NewTask  
###########v1.0.3  
优化部分性能  
再度封装了pushFile和pullFile  
简单处理pullfile粘包  
###########v1.0.2  
去掉无用代码优化部分地方  
涵盖所有AndoridBot接口  
