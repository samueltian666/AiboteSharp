###########环境依赖  
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
!!!所以需要重写Tcp下的connected和recive  
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
        tcp.ais[0].home();  
    }  
}  
```
###########免责声明  
本工具用于个人学习使用，企业自查，严禁使用本工具对互联网造成破坏，感谢。  
###########v1.0.2  
去掉无用代码优化部分地方  
涵盖所有AndoridBot接口  
