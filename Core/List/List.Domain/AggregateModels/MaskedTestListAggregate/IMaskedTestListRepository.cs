using RecAll.Infrastructure.Ddd.Domain.SeedWork;

namespace RecAll.Core.List.Domain.AggregateModels.MaskedTestListAggregate;

public interface IMaskedTestListRepository : IRepository<MaskedTestList>
{
   MaskedTestList Add(MaskedTestList maskedTestList);

    Task<MaskedTestList> GetAsync(int maskedId, string userIdentityGuid);

    Task<IEnumerable<MaskedTestList>> GetMaskedTestListsAsync(IEnumerable<int> maskedIds,
        string userIdentityGuid);
}