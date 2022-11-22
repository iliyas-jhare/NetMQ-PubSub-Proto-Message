using Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;
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

for (int i = 0; i < MessagesCount; i++)
{
    using var poller = new NetMQPoller { socket };

    socket.ReceiveReady += (sender, args) =>
    {
        var message = new NetMQMessage();
        if (args.Socket.TryReceiveMultipartMessage(MessageTimeout, ref message))
        {
            var _ = ToMessage(message);
        }
        else
        {
            Console.WriteLine("Message not recieved.");
        }
    };

    poller.RunAsync();

    Thread.Sleep(MessageInterval);
}

Console.WriteLine("Closing subscriber socket...");
socket.Close();

Console.WriteLine("Press any to continue...");
Console.Read();

static IMessage ToMessage(NetMQMessage msg)
{
    var topic = msg.Pop().ConvertToString();
    using var contents = new CodedInputStream(msg.Pop().ToByteArray());

    var descriptor = MessagesReflection.Descriptor.FindTypeByName<MessageDescriptor>(topic);
    if (descriptor is null)
    {
        throw new ArgumentNullException(nameof(topic), $"Message topic '{topic}' could not be found.");
    }

    var message = descriptor.Parser.ParseFrom(contents);
    if (topic != message.Descriptor.FullName)
    {
        throw new ArgumentException(nameof(topic), $"There is a mismatch in messsage topic. '{topic}'. '{message.Descriptor.FullName}'");
    }

    Console.WriteLine($"Recieved topic '{message.Descriptor.FullName}' with message '{message}'");

    return message;
}
