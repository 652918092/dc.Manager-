using System.ComponentModel.DataAnnotations;

namespace dc.Haiyakj.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}