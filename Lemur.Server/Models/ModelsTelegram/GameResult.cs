using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Lemur.Server.Models.ModelsTelegram;

public class GameResult
{
    [Key] // Marks Id as the primary key
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-generates Id in the database
    public int? Id { get; set; }

    public int UserId { get; set; } // Reference to the Users table

    public string ChosenAnimal { get; set; }

    public string RandomAnimal { get; set; }

    public bool IsWin { get; set; } = false; // Corrected casing for consistency

    public DateTime GameDate { get; set; }
}