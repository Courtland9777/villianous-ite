namespace Villainous.Model;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public record CreateMatchRequest([property: Required, MinLength(2)] IReadOnlyList<string> Villains);
