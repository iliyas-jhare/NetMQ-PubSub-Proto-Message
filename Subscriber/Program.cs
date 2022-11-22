using Common;
using NetMQ;
using NetMQ.Sockets;
using static Common.Configuration;

var topic = Color.Descriptor.FullName;
Console.WriteLine($"Subscriber started for the topic '{topic}'");

using var socket = new SubscriberSocket(ConnectionStr);
socket.Options.ReceiveHighWatermark = HighWatermark;
socket.Subscribe(topic);
Thread.Sleep(MessageInterval);
Console.WriteLine("Subscriber socket created");

var count = 1;
while (count < MessagesCount)
{
    using var poller = new NetMQPoller { socket };
    socket.ReceiveReady += (sender, args) =>
    {
        var mqMessage = new NetMQMessage();
        if (args.Socket.TryReceiveMultipartMessage(MessageTimeout, ref mqMessage))
        {
            var protoMessage = Converter.ToProtoMessage(mqMessage);
            Console.WriteLine($"Recieved topic '{protoMessage.Descriptor.FullName}' with message '{protoMessage}'");
            count++;
        }
#if DEBUG
        else
        {
            Console.WriteLine("Message not recieved.");
        } 
#endif
    };

    poller.RunAsync();
    Thread.Sleep(MessageInterval);
}

Console.WriteLine($"Total {count} messages recieved.");

Console.WriteLine("Closing subscriber socket...");
socket.Close();

Console.WriteLine("Press any to continue...");
Console.Read();
