using System.Text.Json.Nodes;
using MediatR;
using TheSalLab.GeneralReturnValues;

namespace RecAll.Core.List.Api.Application.Commands;

public class CreateMaskedTestListCommand : IRequest<ServiceResult>
{
    public int SetId { get; set; }

    public JsonObject CreateContribJson { get; set; }

    public CreateMaskedTestListCommand(int setId, JsonObject createContribJson) {
        SetId = setId;
        CreateContribJson = createContribJson;
    }
}