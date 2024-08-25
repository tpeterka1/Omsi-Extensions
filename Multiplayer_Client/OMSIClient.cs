using Multiplayer_Client;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Telepathy;

class OMSIClient
{
    
    static void Main(string[] args)
    {
        int playerId = 0;
        Console.Write("Zadejte své ID hráče:");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            if (id < 0)
            {
                Console.WriteLine("Chyba: Musíte zadat platné číselné ID hráče!");
                Environment.Exit(0);
            }
            playerId = id;
        }
        else
        {
            Console.WriteLine("Chyba: Musíte zadat platné číselné ID hráče!");
            Environment.Exit(0);
        }

        string ipaddr = "25.55.37.153";
        //Console.Write("Zadejte IP adresu:");
        //if (IPAddress.TryParse(Console.ReadLine(), out IPAddress ip))
        //{
        //    ipaddr = ip.MapToIPv4().ToString();
        //}
        //else
        //{
        //    if (ip != null && !ip.MapToIPv4().ToString().Equals(""))
        //    {
        //        Console.WriteLine("Chyba: Musíte zadat platnou IP adresu!");
        //        Environment.Exit(0);
        //    }
        //}

        Client client = new Client();
        GameClient gameClient = new GameClient(playerId);
        client.Connect(ipaddr, 12345);


        while (true)
        {
            Telepathy.Message msg;
            while (client.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case EventType.Connected:
                        {
                            Console.WriteLine($"Client connected: {msg.connectionId}");
                            byte[] buff = new byte[4];
                            int out_pos = 0;
                            FastBinaryWriter.Write(buff, ref out_pos, OMSIMPMessages.Messages.REQUEST_VERSION);
                            client.Send(buff);
                            byte[] buff2 = new byte[8];
                            out_pos = 0;
                            gameClient.LastPing = new Tuple<int, long, long>(787, DateTime.Now.Ticks, 0);
                            FastBinaryWriter.Write(buff2, ref out_pos, OMSIMPMessages.Messages.PING);
                            FastBinaryWriter.Write(buff2, ref out_pos, 787);
                            client.Send(buff2);
                        }
                        break;
                    case EventType.Data:
                        MessageParser.ParseMessage(msg.data, client, gameClient);
                        break;
                    case EventType.Disconnected:
                        Console.WriteLine($"Client disconnected: {msg.connectionId}");
                        break;
                }
            }
            gameClient.Tick(client, playerId);
            System.Threading.Thread.Sleep(33);
        }
        client.Disconnect();
    }
}
