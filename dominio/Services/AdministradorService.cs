using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.dominio.entidades;
using minimal_api.Dominio.DTOs;
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

        public Administrador BuscaPorId(int Id)
        {
            return _contexto.administradores.Where(v => v.Id == Id).FirstOrDefault();
        }

        public Administrador Incluir(Administrador administrador)
        {

            _contexto.administradores.Add(administrador);
            _contexto.SaveChanges();

            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.administradores.Where(a => a.Email == loginDTO.email && a.Senha == loginDTO.senha).FirstOrDefault();
            return adm;
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _contexto.administradores.AsQueryable();

            int itensPorPagina = 10;

            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }
    }
}