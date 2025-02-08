using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Lemur.Server.Models.ModelsTelegram
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; } 

        public ICollection<UserTelegram> Users { get; set; }
        public ICollection<GroupPermission> GroupPermissions { get; set; }
    }

}
