using Microsoft.EntityFrameworkCore;
using RecAll.Core.List.Domain.AggregateModels.MaskedTestListAggregate;
using RecAll.Infrastructure.Ddd.Domain.SeedWork;

namespace RecAll.Core.List.Infrastructure.Repositories;

public class MaskedTestListRepository: IMaskedTestListRepository
{
    private readonly ListContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public MaskedTestList Add(MaskedTestList maskedTestList) => _context.MaskedTestLists.Add(maskedTestList).Entity;

    public MaskedTestListRepository(ListContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Update(MaskedTestList maskedTestList) =>
        _context.Entry(maskedTestList).State = EntityState.Modified;

    public async Task<MaskedTestList> GetAsync(int maskedId, string userIdentityGuid)
    {
        var maskedTestList =
            await _context.MaskedTestLists.FirstOrDefaultAsync(p =>
                p.Id == maskedId && p.UserIdentityGuid == userIdentityGuid &&
                !p.IsDeleted) ?? _context.MaskedTestLists.Local.FirstOrDefault(p =>
                p.Id == maskedId && p.UserIdentityGuid == userIdentityGuid &&
                !p.IsDeleted);

        if (maskedTestList is null)
        {
            return null;
        }

        await _context.Entry(maskedTestList).Reference(p => p.Type).LoadAsync();
        return maskedTestList;
    }

    public async Task<IEnumerable<MaskedTestList>> GetMaskedTestListsAsync(IEnumerable<int> maskedIds,
        string userIdentityGuid)
    {
        var maskedTestLists =
            (await _context.MaskedTestLists.Where(p =>
                    maskedIds.Contains(p.Id) &&
                    p.UserIdentityGuid == userIdentityGuid && !p.IsDeleted)
                .ToListAsync()).UnionBy(
                _context.MaskedTestLists.Local.Where(p =>
                       maskedIds.Contains(p.Id) &&
                        p.UserIdentityGuid == userIdentityGuid && !p.IsDeleted)
                    .ToList(), p => p.Id);

        await Task.WhenAll(maskedTestLists.Select(p =>
            _context.Entry(p).Reference(p => p.Type).LoadAsync()));
        return maskedTestLists;
    }
}