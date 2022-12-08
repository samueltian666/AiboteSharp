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
    public static TcpService service;
    public ConcurrentList<Aibote> ais = new();
    Dictionary<string, string> aid_tid = new();
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
            var oai = ais.Where(x => x._aid == aid).FirstOrDefault();
            if (oai != null)
            {
                DestroyTask(oai._client.ID);
            }
            Task.Factory.StartNew(_ => {
                Aibote ai = new(aid, client);
                ais.Add(ai);
                ai.StartScript();
            }, TaskCreationOptions.LongRunning);
            aid_tid[aid] = client.ID;
            Console.WriteLine($"{client.ID}--{aid} connected");
        }
        else
        {
            Console.WriteLine($"已从{client.ID}接收到信息：{mes}");
        }
    }
    public void DestroyTask(string tid)
    {
        var oai = ais.Where(x => x._client.ID == tid).FirstOrDefault();
        if (oai != null)
        {
            oai.Stop();
            oai._client.Close();
            ais.Remove(oai);
            aid_tid.Remove(oai._aid);
        }
    }
}
