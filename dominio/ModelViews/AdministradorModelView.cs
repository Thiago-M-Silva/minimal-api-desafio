using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Enuns;

namespace minimal_api.Dominio.ModelViews
{
    public record AdministradorModelView
    {
        public string Email { get; set; }
        public string Senha { get; set; }
        public Perfil Perfil { get; set; }

    }
}