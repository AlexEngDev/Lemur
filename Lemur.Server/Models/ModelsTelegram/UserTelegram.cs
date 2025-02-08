using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Lemur.Server.Models.ModelsTelegram;

public class UserTelegram
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }

    public string Username { get; set; }

    public int GroupId { get; set; }

    public long? TelegramId { get; set; }

    public long? TelegramIdChat { get; set; }

    public string? PhoneNumber { get; set; }

    public bool isAuthorized { get; set; } = true;

    [ForeignKey("GroupId")]
    public Group Group { get; set; }
}
