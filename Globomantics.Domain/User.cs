namespace Globomantics.Domain;

public record User(string Name)
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
