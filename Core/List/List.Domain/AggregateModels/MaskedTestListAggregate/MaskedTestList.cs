using RecAll.Core.List.Domain.Events;
using RecAll.Core.List.Domain.Exceptions;
using RecAll.Infrastructure.Ddd.Domain.SeedWork;

namespace RecAll.Core.List.Domain.AggregateModels.MaskedTestListAggregate;

public class MaskedTestList : Entity, IAggregateRoot {
    private int _typeId;
    public ListType Type { get; private set; }

    private int _setId;

    public int SetId => _setId;

    private string _contribId;

    public string ContribId => _contribId;

    private string _userIdentityGuid;

    public string UserIdentityGuid => _userIdentityGuid;

    private bool _isDeleted;

    public bool IsDeleted => _isDeleted;

    private MaskedTestList() { }

    public MaskedTestList(int typeId, int setId, string contribId,
        string userIdentityGuid) : this() {
        _typeId = typeId;
        _setId = setId;
        _contribId = contribId;
        _userIdentityGuid = userIdentityGuid;

        var maskedTestListCreatedDomainEvent = new MaskedTestListCreatedDomainEvent(this);
        AddDomainEvent(maskedTestListCreatedDomainEvent);
    }

    public void SetSetId(int setId) {
        if (_isDeleted) {
            ThrowDeletedException();
        }

        _setId = setId;
    }

    public void SetDeleted() {
        if (_isDeleted) {
            ThrowDeletedException();
        }

        _isDeleted = true;

        var maskedTestListDeletedDomainEvent = new MaskedTestListDeletedDomainEvent(this);
        AddDomainEvent(maskedTestListDeletedDomainEvent);
    }

    private void ThrowDeletedException() =>
        throw new ListDomainException("项目已删除。");
}