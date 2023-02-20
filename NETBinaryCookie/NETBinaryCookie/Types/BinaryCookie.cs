using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NETBinaryCookie.Types;

public class BinaryCookie
{
    public static bool operator ==(BinaryCookie left, BinaryCookie right) =>
        string.Equals(left.Domain, right.Domain, StringComparison.CurrentCultureIgnoreCase) &&
        string.Equals(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase) &&
        string.Equals(left.Path, right.Path, StringComparison.CurrentCultureIgnoreCase);
    
    public static bool operator !=(BinaryCookie left, BinaryCookie right) => !(left == right);

    public override string ToString()
    {
        return $"DOMAIN: {this.Domain}\nNAME: {this.Name}\nPATH: {this.Path}\n" +
               $"FLAGS: {(this.Flags.Length > 0 ? string.Join(", ", this.Flags) : "(none)")}\n" +
               $"SET: {this.Creation}\nEXPIRES: {this.Expiration}\nVALUE: {this.Value}\n";
    }

    [Required] public DateTime Expiration { get; set; }
    
    [Required] public DateTime Creation { get; set; }
    
    public string? Comment { get; set; }
    
    [Required] public string Domain { get; set; } = string.Empty;
    
    [Required] public string Name { get; set; } = string.Empty;
    
    [Required] public string Path { get; set; } = string.Empty;
    
    [Required] public string Value { get; set; } = string.Empty;
    
    [Required] [JsonConverter(typeof(CookieFlagConverter))]
    public NetBinaryCookie.CookieFlag[] Flags { get; set; } = Array.Empty<NetBinaryCookie.CookieFlag>();
}