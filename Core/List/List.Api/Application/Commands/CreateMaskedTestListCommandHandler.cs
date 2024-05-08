using System.Text.Json;
using Infrastructure.Api.HttpClient;
using MediatR;
using RecAll.Core.List.Api.Application.IntegrationEvents;
using RecAll.Core.List.Api.Application.Queries;
using RecAll.Core.List.Api.Infrastructure.Services;
using RecAll.Core.List.Domain.AggregateModels.MaskedTestListAggregate;
using TheSalLab.GeneralReturnValues;

namespace RecAll.Core.List.Api.Application.Commands;

public class CreateMaskedTestListCommandHandler: IRequestHandler<CreateMaskedTestListCommand,
    ServiceResult> {
    private ISetQueryService _setQueryService;
    private readonly IIdentityService _identityService;
    private readonly IContribUrlService _contribUrlService;
    private readonly HttpClient _httpClient;
    private readonly IMaskedTestListRepository _maskedTestListRepository;
    private readonly IListIntegrationEventService _listIntegrationEventService;

    public CreateMaskedTestListCommandHandler(ISetQueryService setQueryService,
        IIdentityService identityService, IMaskedTestListRepository maskedTestListRepository,
        IContribUrlService contribUrlService,
        IHttpClientFactory httpClientFactory,
        IListIntegrationEventService listIntegrationEventService) {
        _setQueryService = setQueryService ??
            throw new ArgumentNullException(nameof(setQueryService));
        _identityService = identityService ??
            throw new ArgumentNullException(nameof(identityService));
        _maskedTestListRepository = maskedTestListRepository ??
            throw new ArgumentNullException(nameof(maskedTestListRepository));
        _contribUrlService = contribUrlService;
        _httpClient = httpClientFactory.CreateDefaultClient();
        _listIntegrationEventService = listIntegrationEventService ??
            throw new ArgumentNullException(
                nameof(listIntegrationEventService));
    }

    public async Task<ServiceResult> Handle(CreateMaskedTestListCommand command,
        CancellationToken cancellationToken) {
        var set = await _setQueryService.GetAsync(command.SetId,
            _identityService.GetUserIdentityGuid());
        var contribUrl =
            $"{_contribUrlService.GetContribUrl(set.TypeId)}/MaskedTestList/create";

        var jsonContent = JsonContent.Create(command.CreateContribJson,
            options: new JsonSerializerOptions {
                PropertyNameCaseInsensitive = false
            });

        HttpResponseMessage response;
        try {
            response = await _httpClient.PostAsync(contribUrl, jsonContent,
                cancellationToken);
            response.EnsureSuccessStatusCode();
        } catch (Exception e) {
            return ServiceResult.CreateExceptionResult(e,
                $"访问Contrib Url时发生错误。TypeId: {set.TypeId}, ContribUrl: {contribUrl}");
        }

        var responseJson =
            await response.Content.ReadAsStringAsync(cancellationToken);
        var contribResult = JsonSerializer
            .Deserialize<ServiceResultViewModel<string>>(responseJson,
                new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                }).ToServiceResult();

        if (contribResult.Status != ServiceResultStatus.Succeeded) {
            return contribResult;
        }

        var maskedTestList = _maskedTestListRepository.Add(new MaskedTestList(set.TypeId, command.SetId,
            contribResult.Result, _identityService.GetUserIdentityGuid()));
        var saved =
            await _maskedTestListRepository.UnitOfWork.SaveEntitiesAsync(
                cancellationToken);

        if (!saved) {
            return ServiceResult.CreateFailedResult();
        }

        var maskedTestListIdAssignedIntegrationEvent =
            new MaskedTestListIdAssignedIntegrationEvent(set.TypeId, contribResult.Result,
                maskedTestList.Id);
        await _listIntegrationEventService.AddAndSaveEventAsync(
            maskedTestListIdAssignedIntegrationEvent);

        return await _maskedTestListRepository.UnitOfWork.SaveEntitiesAsync(
            cancellationToken)
            ? ServiceResult.CreateSucceededResult()
            : ServiceResult.CreateFailedResult();
    }
}