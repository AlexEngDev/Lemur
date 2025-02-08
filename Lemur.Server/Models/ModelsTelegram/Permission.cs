using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Lemur.Server.Models.ModelsTelegram
{
    public class Permission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Command { get; set; }

        public string? Description { get; set; }
        public string? Response { get; set; } //se esistae la risposta dal DB

        public bool IsDefault { get; set; } = false;

        public ICollection<GroupPermission> GroupPermissions { get; set; }

    }
}
