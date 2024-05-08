using System.ComponentModel.DataAnnotations;

namespace RecAll.Contrib.MaksedTestList.Api.Commands;

public class GetMaskedTestListsCommand
{
    [Required] public IEnumerable<int> Ids { get; set; }
}