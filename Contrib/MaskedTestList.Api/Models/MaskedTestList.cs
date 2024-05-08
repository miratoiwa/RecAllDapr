namespace RecAll.Contrib.MaksedTestList.Api.Models;

public class MaskedTestList
{
    public int Id { get; set; }

    public int? MaskedId { get; set; }

    public string Content { get; set; }
    
    public string MaskedContent { get; set; }

    public string UserIdentityGuid { get; set; }

    public bool IsDeleted { get; set; }
}