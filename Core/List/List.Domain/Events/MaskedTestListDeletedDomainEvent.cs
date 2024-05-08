using MediatR;
using RecAll.Core.List.Domain.AggregateModels.MaskedTestListAggregate;

namespace RecAll.Core.List.Domain.Events;

public class MaskedTestListDeletedDomainEvent: INotification
{
    public MaskedTestList MaskedTestList { get; set; }

    public MaskedTestListDeletedDomainEvent(MaskedTestList maskedTestList)
    {
        MaskedTestList = maskedTestList;
    }
}