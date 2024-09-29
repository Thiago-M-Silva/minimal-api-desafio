using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.dominio.entidades;
using minimal_api.infraestrutura.Interfaces;
using MinimalApi.DTOs;
using MinimalAPI.Infraestrutura.DB;

namespace minimal_api.dominio.services
{
    public class AdministradorService : iAdministradorService
    {
        private readonly DBContexto _contexto;

        public AdministradorService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.administradores.Where(a => a.Email == loginDTO.email && a.Senha == loginDTO.senha).FirstOrDefault();
            return adm;
        }
    }
}