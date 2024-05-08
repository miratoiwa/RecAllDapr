using MediatR;
using RecAll.Core.List.Domain.AggregateModels.MaskedTestListAggregate;

namespace RecAll.Core.List.Domain.Events;

public class MaskedTestListCreatedDomainEvent: INotification
{
    public MaskedTestList MaskedTestList { get; }

    public MaskedTestListCreatedDomainEvent(MaskedTestList maskedTestList)
    {
        MaskedTestList = maskedTestList;
    }
}