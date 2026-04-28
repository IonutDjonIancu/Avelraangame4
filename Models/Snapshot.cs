using Statics;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class Snapshot
{
    [MaxLength(10, ErrorMessage = Errors.MaxNumberOfPlayers)]
    public List<Player> Players { get; set; } = [];
}
