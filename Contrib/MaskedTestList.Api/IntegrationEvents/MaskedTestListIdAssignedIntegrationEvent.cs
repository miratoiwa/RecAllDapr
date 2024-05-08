using RecAll.Infrastructure.EventBus.Events;

namespace RecAll.Contrib.MaksedTestList.Api;

public record MaskedTestListIdAssignedIntegrationEvent(
    int MaskedId,
    int TypeId,
    string ContribId) : IntegrationEvent;