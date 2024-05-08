﻿using RecAll.Core.List.Domain.AggregateModels;
using RecAll.Infrastructure.Ddd.Domain.SeedWork;

namespace RecAll.Core.List.Api.Infrastructure.Services;

public class ContribUrlService : IContribUrlService {
    public string GetContribUrl(int listTypeId) {
        string route;

        if (listTypeId == ListType.Text.Id) {
            route = "text";
        }else if(listTypeId == ListType.MaskedTestList.Id)
        {
            route = "masked";
        } else {
            throw new ArgumentOutOfRangeException(nameof(listTypeId),
                $"有效取值为{string.Join(",", Enumeration.GetAll<ListType>().Select(p => p.Id.ToString()))}");
        }

        return $"http://recall-envoygateway/{route}";
    }
}