﻿using RecAll.Core.List.Domain.Exceptions;
using RecAll.Infrastructure.Ddd.Domain.SeedWork;

namespace RecAll.Core.List.Domain.AggregateModels;

public class ListType : Enumeration {
    public const int TextId = 1;

    public const int MaskedId = 2;
    
    public static ListType Text = new(TextId, nameof(Text).ToLowerInvariant(),
        nameof(Text));

    public static ListType MaskedTestList = new(MaskedId, nameof(MaskedTestList).ToLowerInvariant(),
        nameof(MaskedTestList));
    public ListType(int id, string name, string displayName) : base(id, name,
        displayName) { }

    private static ListType[] _list = { Text ,MaskedTestList };

    public static IEnumerable<ListType> List() => _list;

    public static ListType FromName(string name) =>
        List().SingleOrDefault(p => string.Equals(p.Name, name,
            StringComparison.CurrentCultureIgnoreCase)) ??
        throw new ListDomainException(
            $"Possible values for {nameof(ListType)}: {string.Join(",", List().Select(p => p.Name))}");
}