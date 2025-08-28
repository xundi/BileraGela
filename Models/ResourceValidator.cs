using Reservas.Models;

// Models/ResourceValidator.cs
public class ResourceValidator
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int? CenterId { get; set; }
    public Center? Center { get; set; }

    public int? ResourceId { get; set; }
    public Resource? Resource { get; set; }
}
