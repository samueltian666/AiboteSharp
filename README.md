###########环境依赖  
dotnet6+touchsocket  
###########touchsocket文档  
https://www.yuque.com/rrqm/touchsocket/55e5bbf58745fa639dba511c7bcd54d1  
###########aibote协议文档  
http://www.aibote.net/aiboteProtocol.html  
###########v1.0.0  
基本涵盖AndoridBot  
###########使用  
Tcp tcp = new("0.0.0.0:5211");  
while (true)  
{  
    string v = Console.ReadLine();  
    if (v == "1")  
    {  
        //tcp.ais[0].AddMisson("startApp",new object[] { "com.android.settings" });  
        tcp.ais[0].home();  
    }  
}  
###########欢迎贡献  
https://github.com/samueltian666/AiboteSharp  
###########免责声明  
本工具用于个人学习使用，企业自查，严禁使用本工具对互联网造成破坏，感谢。  