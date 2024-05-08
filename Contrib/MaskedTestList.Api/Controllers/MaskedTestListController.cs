using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecAll.Contrib.MaksedTestList.Api.Commands;
using RecAll.Contrib.MaksedTestList.Api.Services;
using RecAll.Contrib.MaksedTestList.Api.ViewModels;
using TheSalLab.GeneralReturnValues;

namespace RecAll.Contrib.MaksedTestList.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class MaskedTestListController
{
     private readonly IIdentityService _identityService;
    private readonly MaskedTestListContext _maskedTestListContext;
    private readonly ILogger<MaskedTestListController> _logger;

    public MaskedTestListController(IIdentityService identityService,
        MaskedTestListContext maskedTestListContext, ILogger<MaskedTestListController> logger) {
        _identityService = identityService;
        _maskedTestListContext = maskedTestListContext;
        _logger = logger;
    }

    [Route("create")]
    [HttpPost]
    public async Task<ActionResult<ServiceResultViewModel<string>>> CreateAsync(
        [FromBody] CreateMaskedTestListCommand command) {
        _logger.LogInformation(
            "----- Handling command {CommandName} ({@Command})",
            command.GetType().Name, command);

        var maskedTestList = new Models.MaskedTestList {
            Content = command.Content,
            MaskedContent = command.MaskedContent,
            UserIdentityGuid = _identityService.GetUserIdentityGuid(),
            IsDeleted = false
        };
        var maskedTestListEntity = _maskedTestListContext.Add(maskedTestList);
        await _maskedTestListContext.SaveChangesAsync();

        _logger.LogInformation("----- Command {CommandName} handled",
            command.GetType().Name);

        return ServiceResult<string>
            .CreateSucceededResult(maskedTestListEntity.Entity.Id.ToString())
            .ToServiceResultViewModel();
    }

    [Route("update")]
    [HttpPost]
    public async Task<ServiceResultViewModel> UpdateAsync(
        [FromBody] UpdateMaskedTestListCommand command) {
        _logger.LogInformation(
            "----- Handling command {CommandName} ({@Command})",
            command.GetType().Name, command);

        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var maskedTestList = await _maskedTestListContext.MaskedTestLists.FirstOrDefaultAsync(p =>
            p.Id == command.Id && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);

        if (maskedTestList is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的MaskedTestList {command.Id}");

            return ServiceResult
                .CreateFailedResult($"Unknown MaskedTestList id: {command.Id}")
                .ToServiceResultViewModel();
        }

        maskedTestList.Content = command.Content;
        maskedTestList.MaskedContent = command.MaskedContent;
        await _maskedTestListContext.SaveChangesAsync();

        _logger.LogInformation("----- Command {CommandName} handled",
            command.GetType().Name);

        return ServiceResult.CreateSucceededResult().ToServiceResultViewModel();
    }

    [Route("get/{id}")]
    [HttpGet]
    public async Task<ActionResult<ServiceResultViewModel<MaskedTestListViewModel>>>
        GetAsync(int id) {
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var maskedTestList = await _maskedTestListContext.MaskedTestLists.FirstOrDefaultAsync(p =>
            p.Id == id && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);

        if (maskedTestList is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的MaskedTestList {id}");

            return ServiceResult<MaskedTestListViewModel>
                .CreateFailedResult($"Unknown MaskedTestList id: {id}")
                .ToServiceResultViewModel();
        }

        return ServiceResult<MaskedTestListViewModel>
            .CreateSucceededResult(new MaskedTestListViewModel {
                Id = maskedTestList.Id,
                MaskedId = maskedTestList.MaskedId,
                MaskedContent = maskedTestList.MaskedContent,
                Content = maskedTestList.Content
            }).ToServiceResultViewModel();
    }

    [Route("getByMaskedId/{maskedId}")]
    [HttpGet]
    // ServiceResultViewModel<MaskedTestListViewModel>
    public async Task<ActionResult<ServiceResultViewModel<MaskedTestListViewModel>>>
        GetByMaskedId(int maskedId) {
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var maskedTestList = await _maskedTestListContext.MaskedTestLists.FirstOrDefaultAsync(p =>
            p.MaskedId == maskedId && p.UserIdentityGuid == userIdentityGuid &&
            !p.IsDeleted);

        if (maskedTestList is null) {
            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的MaskedTestList, MaskedID: {maskedId}");

            return ServiceResult<MaskedTestListViewModel>
                .CreateFailedResult($"Unknown MaskedTestList with MaskedID: {maskedId}")
                .ToServiceResultViewModel();
        }

        return ServiceResult<MaskedTestListViewModel>
            .CreateSucceededResult(new MaskedTestListViewModel {
                Id = maskedTestList.Id,
                MaskedId = maskedTestList.MaskedId,
                Content = maskedTestList.Content,
                MaskedContent = maskedTestList.MaskedContent
            }).ToServiceResultViewModel();
    }

    [Route("getMaskedTestLists")]
    [HttpPost]
    public async
        Task<ActionResult<
            ServiceResultViewModel<IEnumerable<MaskedTestListViewModel>>>>
        GetMaskedTestListsAsync(GetMaskedTestListsCommand command) {
        var maskedIds = command.Ids.ToList();
        var userIdentityGuid = _identityService.GetUserIdentityGuid();

        var maskedTestLists = await _maskedTestListContext.MaskedTestLists.Where(p =>
                p.MaskedId.HasValue && maskedIds.Contains(p.MaskedId.Value) &&
                p.UserIdentityGuid == userIdentityGuid && !p.IsDeleted)
            .ToListAsync();

        if (maskedTestLists.Count != maskedIds.Count) {
            var missingIds = string.Join(",",
                maskedIds.Except(maskedTestLists.Select(p => p.MaskedId.Value))
                    .Select(p => p.ToString()));

            _logger.LogWarning(
                $"用户{userIdentityGuid}尝试查看已删除、不存在或不属于自己的MaskedTestList {missingIds}");

            return ServiceResult<IEnumerable<MaskedTestListViewModel>>
                .CreateFailedResult($"Unknown MaskedTestList id: {missingIds}")
                .ToServiceResultViewModel();
        }

        maskedTestLists.Sort((x, y) =>
            maskedIds.IndexOf(x.MaskedId.Value) - maskedIds.IndexOf(y.MaskedId.Value));

        return ServiceResult<IEnumerable<MaskedTestListViewModel>>
            .CreateSucceededResult(maskedTestLists.Select(p => new MaskedTestListViewModel {
                Id = p.Id, MaskedId = p.MaskedId, Content = p.Content, MaskedContent = p.MaskedContent
            })).ToServiceResultViewModel();
    }
    
}