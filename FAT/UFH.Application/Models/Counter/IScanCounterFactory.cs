

namespace UFH.Application.Models
{
    public interface IScanCounterFactory
    {
        IScanCounter CreateCounterByCounterOrder(CounterOrder counterOrder);
    }
}
