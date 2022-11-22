using Common;
using Google.Protobuf;
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

    if (socket.TrySendMultipartMessage(MessageTimeout, ToMessage(color)))
    {
        Console.WriteLine($"Sent topic '{topic}' with message '{color}'");
    }
    else
    {
        Console.WriteLine("Message not sent.");
    }

    Thread.Sleep(MessageInterval);
}

Console.WriteLine("Closing publisher socket...");
socket.Close();

Console.WriteLine("Press any to continue...");
Console.Read();

static NetMQMessage ToMessage(IMessage data)
{
    using var memStream = new MemoryStream();
    using var outStream = new CodedOutputStream(memStream);
    data.WriteTo(outStream);
    outStream.Flush();

    var message = new NetMQMessage();
    message.Append(data.Descriptor.FullName);
    message.Append(memStream.ToArray());

    return message;
}

static int GetNumber(Random r) => r.Next(0, 256);