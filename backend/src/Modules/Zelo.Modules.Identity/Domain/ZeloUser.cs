using Microsoft.AspNetCore.Identity;

namespace Zelo.Modules.Identity.Domain;

internal sealed class ZeloUser : IdentityUser<Guid>
{
    /// Nome apresentado na app (sidebar, perfil). Opcional - o registo
    /// (MapIdentityApi, sem gancho para campos extra) nao o define; fica a
    /// null ate o utilizador o definir na pagina de perfil, altura em que a
    /// UI mostra o email.
    public string? Name { get; set; }
}
