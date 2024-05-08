using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecAll.Core.List.Api.Application.Commands;
using RecAll.Core.List.Api.Infrastructure.Services;
using TheSalLab.GeneralReturnValues;

namespace RecAll.Core.List.Api.Controllers;


[ApiController]
[Authorize]
[Route("[controller]")]
public class MaskedTestListController
{
    private readonly IMediator _mediator;
    private readonly IIdentityService _identityService;
    private readonly ILogger<MaskedTestListController> _logger;

    public MaskedTestListController(IMediator mediator, IIdentityService identityService,
        ILogger<MaskedTestListController> logger)
    {
        _mediator = mediator ??
                    throw new ArgumentNullException(nameof(mediator));
        _identityService = identityService ??
                           throw new ArgumentNullException(
                               nameof(identityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Route("create")]
    [HttpPost]
    public async Task<ActionResult<ServiceResultViewModel>> CreateAsync(
        CreateMaskedTestListCommand command)
    {
        _logger.LogInformation(
            "----- Sending command: {CommandName} - UserIdentityGuid: {userIdentityGuid} ({@Command})",
            command.GetType().Name, _identityService.GetUserIdentityGuid(),
            command);

        return (await _mediator.Send(command)).ToServiceResultViewModel();
    }
}