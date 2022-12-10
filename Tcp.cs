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
        service.Setup(new TouchSocketConfig().SetListenIPHosts(new IPHost[] { new IPHost(ip) }).SetClearInterval(-1).SetBufferLength(1024 * 1024));
        service.Start();
        service.Connected+= connected;
        service.Received += receive;
        if (service.ServerState == ServerState.Running)
        {
            Console.WriteLine("tcp is running");
        }
    }
    public virtual void connected(SocketClient client,TouchSocketEventArgs e)
    {
        client.SendAsync(Helper.CombineWithParams("getAndroidId"));
    }
    public virtual void receive(SocketClient client,ByteBlock byteBlock ,IRequestInfo info)
    {
        for (int i = 0; i < byteBlock.Buffer.Length; i++)
        {
            //const byte b = 47;
            if (byteBlock.Buffer[i] == 47 && i > 1)
            {
                byte[] n = byteBlock.Buffer.Skip(i + 1).Take(byteBlock.Len - (i + 1)).ToArray();
                if (n.Length == 16)
                {
                    string aid = Encoding.UTF8.GetString(n);
                    var oai = ais.FirstOrDefault(x => x._aid == aid);
                    if (oai != null)
                    {
                        ForceDestroyTask(oai._aid);
                    }
                    Task.Factory.StartNew(_ => {
                        Aibote ai = new(aid, client);
                        ais.Add(ai);
                        ai.Start();
                    }, TaskCreationOptions.LongRunning);
                    Console.WriteLine($"{client.ID}--{aid} connected");
                }
                return;
            }
            if (byteBlock.Buffer[i] < 48 || byteBlock.Buffer[i] > 57)
            {
                var oai = ais.FirstOrDefault(x => x._client.ID == client.ID);
                oai?.CombineByte(byteBlock.Buffer.Take(byteBlock.Len).ToArray());
                return;
            }
            if (i > 4) break;
        }
    }
    public virtual void ForceDestroyTask(string aid)
    {
        var oai = ais.FirstOrDefault(x => x._aid == aid);
        if (oai != null)
        {
            oai.ForceDestroyTask();
            ais.Remove(oai);
        }
    }
    public virtual void MissonClear(string aid)
    {
        var oai = ais.FirstOrDefault(x => x._aid == aid);
        oai?.MissonClear();
    }
}
