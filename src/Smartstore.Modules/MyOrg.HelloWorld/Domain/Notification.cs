using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smartstore.Domain;

[Table("Notification")]
[Index(nameof(AuthorId), Name = "IX_Notification_AuthorId")]
[Index(nameof(Published), Name = "IX_Notification_Published")]
public class Notification : BaseEntity
{
    public int AuthorId { get; set; }

    public DateTime Published { get; set; }

    [MaxLength]
    public string Message { get; set; } = string.Empty;
}
