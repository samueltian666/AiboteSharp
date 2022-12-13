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
    public Tcp(string ip, string servername = "111")
    {
        service = new();
        service.Setup(new TouchSocketConfig()
            .SetListenIPHosts(new IPHost[] { new IPHost(ip) })
            .SetClearInterval(-1)
            .SetBufferLength(1024 * 1024)
            .SetServerName(servername));
        service.Connected += connected;
        service.Received += received;
        service.Start();
        if (service.ServerState == ServerState.Running)
        {
            Console.WriteLine("tcp is running");
        }
    }
    protected virtual void connected(SocketClient client, TouchSocketEventArgs e) => client.SendAsync(Helper.CombineWithParams("getAndroidId"));
    protected virtual void received(SocketClient client, ByteBlock byteBlock, IRequestInfo info)
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
                    NewTask(client, aid);
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
    protected virtual void NewTask(SocketClient client, string aid)
    {
        Task.Factory.StartNew(_ =>
        {
            Aibote ai = new(aid, client);
            ais.Add(ai);
            ai.Start();
        }, TaskCreationOptions.LongRunning);
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
