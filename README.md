###########环境依赖  
dotnet6+touchsocket  
###########touchsocket文档  
https://www.yuque.com/rrqm/touchsocket/55e5bbf58745fa639dba511c7bcd54d1  
###########aibote协议文档  
http://www.aibote.net/aiboteProtocol.html  
###########v1.0.0  
基本涵盖AndoridBot的接口带详细返回  
开放接口可调用不限于AndoridBot的接口  
```c#  
AddMisson("findWindow","className", "windowName");  
AddMisson("getClipboardText")
//注意addmisson返回目前需要自行处理
```
欢迎贡献提bug完善框架    
###########使用  
###########nuget  
https://www.nuget.org/packages/AiboteSharp/  
直接nuget引用或者添加publish下的AiboteSharp.dll和TouchSocket.dll即可  
```c#
Tcp tcp = new("0.0.0.0:5211");  
while (true)  
{  
    string v = Console.ReadLine();  
    if (v == "1")  
    {  
        //tcp.ais[0].AddMisson("startApp","com.android.settings");  
        //tcp.ais[0].AddMisson("home")  
        //注意addmisson返回目前需要自行处理
        tcp.ais[0].home();  
    }  
}  
```

###########欢迎贡献  
https://github.com/samueltian666/AiboteSharp  
###########免责声明  
本工具用于个人学习使用，企业自查，严禁使用本工具对互联网造成破坏，感谢。  