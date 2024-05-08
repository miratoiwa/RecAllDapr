using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecAll.Contrib.MaksedTestList.Api.Services;
using RecAll.Core.List.Domain.AggregateModels;
using RecAll.Infrastructure.EventBus;

namespace RecAll.Contrib.MaksedTestList.Api.Controllers;


[ApiController]
[Route("[controller]")]
public class MaskedTestListIdAssignedIntegrationEventController
{
    private readonly MaskedTestListContext _maskedTestListContext;

    private readonly ILogger<MaskedTestListIdAssignedIntegrationEventController> _logger;

    public MaskedTestListIdAssignedIntegrationEventController(
       MaskedTestListContext maskedTestListContext,
        ILogger<MaskedTestListIdAssignedIntegrationEventController> logger)
    {
        _maskedTestListContext = maskedTestListContext;
        _logger = logger;
    }

    [Route("maskedIdAssigned")]
    [HttpPost]
    [Topic(DaprEventBus.PubSubName, nameof(MaskedTestListIdAssignedIntegrationEvent))]
    public async Task HandleAsync(MaskedTestListIdAssignedIntegrationEvent @event) {
        if (@event.TypeId != ListType.MaskedTestList.Id) {
            return;
        }

        _logger.LogInformation(
            "----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
            @event.Id, ProgramExtensions.AppName, @event);

        var maskedTestList = await _maskedTestListContext.MaskedTestLists.FirstOrDefaultAsync(p =>
            p.Id == int.Parse(@event.ContribId));

        if (maskedTestList is null) {
            _logger.LogWarning("Unknown MaskedTestList id: {MaskedId}", @event.MaskedId);
            return;
        }

        maskedTestList.MaskedId = @event.MaskedId;
        await _maskedTestListContext.SaveChangesAsync();

        _logger.LogInformation(
            "----- Integration event handled: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
            @event.Id, ProgramExtensions.AppName, @event);
    }
}