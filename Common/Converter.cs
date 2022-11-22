using Google.Protobuf;
using Google.Protobuf.Reflection;
using NetMQ;

namespace Common;

public static class Converter
{
    public static IMessage ToProtoMessage(NetMQMessage message)
    {
        var topic = message.Pop().ConvertToString();
        using var contents = new CodedInputStream(message.Pop().ToByteArray());

        var descriptor = MessagesReflection.Descriptor.FindTypeByName<MessageDescriptor>(topic);
        if (descriptor is null)
        {
            throw new ArgumentException($"Message topic '{topic}' could not be found.");
        }

        var instance = descriptor.Parser.ParseFrom(contents);
        if (topic != instance.Descriptor.FullName)
        {
            throw new ArgumentException($"There is a mismatch in the messsage topic. '{topic}'. '{instance.Descriptor.FullName}'");
        }

        return instance;
    }

    public static NetMQMessage ToNetMQMessage(IMessage message)
    {
        using var contents = new MemoryStream();
        using var outStream = new CodedOutputStream(contents);
        message.WriteTo(outStream);
        outStream.Flush();

        var instance = new NetMQMessage();
        instance.Append(message.Descriptor.FullName);
        instance.Append(contents.ToArray());

        return instance;
    }
}
