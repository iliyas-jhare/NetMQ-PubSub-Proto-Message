using Common;
using NetMQ;
using NetMQ.Sockets;
using static Common.Configuration;

var topic = Color.Descriptor.FullName;
Console.WriteLine($"Publisher started for the topic '{topic}'");

var rand = new Random();

using var socket = new PublisherSocket(ConnectionStr);
socket.Options.SendHighWatermark = HighWatermark;
Thread.Sleep(MessageInterval);
Console.WriteLine("Publisher socket created");

for (var i = 0; i < MessagesCount; i++)
{
    var color = new Color
    {
        RED = GetNumber(rand),
        GREEN = GetNumber(rand),
        BLUE = GetNumber(rand)
    };

    if (socket.TrySendMultipartMessage(MessageTimeout, Converter.ToNetMQMessage(color)))
    {
        Console.WriteLine($"Sent topic '{topic}' with message '{color}'");
    }
#if DEBUG
    else
    {
        Console.WriteLine("Message not sent.");
    }
#endif

    Thread.Sleep(MessageInterval);
}

Console.WriteLine($"Total {MessagesCount} messages sent.");

Console.WriteLine("Closing publisher socket...");
socket.Close();

Console.WriteLine("Press any to continue...");
Console.Read();

static int GetNumber(Random r) => r.Next(0, 256);