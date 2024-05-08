using System.ComponentModel.DataAnnotations;

namespace RecAll.Contrib.MaksedTestList.Api.Commands;

public class CreateMaskedTestListCommand
{
    [Required] public string Content { get; set; }
    
    [Required] public string MaskedContent { get; set; }
}