

namespace FAT.Application.Models
{
    public interface IScanCounterFactory
    {
        IScanCounter CreateCounterByCounterOrder(CounterOrder counterOrder);
    }
}
