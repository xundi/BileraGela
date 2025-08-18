using Reservas.Models;

public class ResourceValidator
{
    public int Id { get; set; }

    public int ResourceId { get; set; }
    public Resource Resource { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
}
