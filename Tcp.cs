using System.Net.Sockets;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Collections.Concurrent;
using TouchSocket.Core.Config;
using TouchSocket.Sockets;

namespace AiboteSharp;

public class Tcp
{
    public TcpService service;
    public ConcurrentList<Aibote> ais = new();
    const byte b = 47;
    public Tcp(string ip)
    {
        service = new();
        service.Setup(new TouchSocketConfig().SetListenIPHosts(new IPHost[] { new IPHost(ip) }).SetClearInterval(-1));
        service.Start();
        service.Connected+= connected;
        service.Received += recive;
        if (service.ServerState == ServerState.Running)
        {
            Console.WriteLine("tcp is running");
        }
    }
    public virtual void connected(SocketClient client,TouchSocketEventArgs e)
    {
        client.SendAsync(Helper.CombineWithParams("getAndroidId"));
    }
    public virtual void recive(SocketClient client,ByteBlock byteBlock ,IRequestInfo info)
    {
        string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
        int index = 1;
        for (int i = 0; i < byteBlock.Buffer.Length; i++)
        {
            if (byteBlock.Buffer[i] == b)
            {
                index += i;
                break;
            }
        }
        byte[] n = byteBlock.Buffer.Skip(index).Take(byteBlock.Len - index).ToArray();
        if (n.Length == 16)
        {
            string aid = Encoding.UTF8.GetString(n);
            var oai = ais.FirstOrDefault(x => x._aid == aid);
            if (oai != null)
            {
                ForceDestroyTask(oai._client.ID);
            }
            Task.Factory.StartNew(() => {
                Aibote ai = new(aid, client);
                ais.Add(ai);
                ai.Start();
            }, TaskCreationOptions.LongRunning);
            Console.WriteLine($"{client.ID}--{aid} connected");
        }
        else
        {
            Console.WriteLine($"新创建aibote线程处收到不该出现的消息{client.ID}接收到信息：{mes}");
        }
    }
    public virtual void ForceDestroyTask(string aid)
    {
        var oai = ais.FirstOrDefault(x => x._aid == aid);
        if (oai != null)
        {
            oai.ForceDestroyTask();
            oai._client.Close();
            ais.Remove(oai);
        }
    }
    public virtual void MissonClear(string aid)
    {
        var oai = ais.FirstOrDefault(x => x._aid == aid);
        if (oai != null)
        {
            oai.MissonClear();
        }
    }
}
