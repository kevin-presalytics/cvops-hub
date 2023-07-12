using System.Threading.Channels;

namespace lib.services
{
    public interface IChannelOwner<T>
    {
        ChannelReader<T> ChannelReader {get;}
        ChannelWriter<T> ChannelWriter {get;}
    }
}