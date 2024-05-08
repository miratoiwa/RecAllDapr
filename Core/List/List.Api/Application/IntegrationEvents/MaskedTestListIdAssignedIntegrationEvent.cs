using RecAll.Infrastructure.EventBus.Events;

namespace RecAll.Core.List.Api.Application.IntegrationEvents;

public record MaskedTestListIdAssignedIntegrationEvent(
    int typeId,
    string contribId,
    int maskedId) : IntegrationEvent;