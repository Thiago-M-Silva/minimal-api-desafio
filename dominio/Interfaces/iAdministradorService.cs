using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.dominio.entidades;
using MinimalApi.DTOs;

namespace minimal_api.infraestrutura.Interfaces
{
    public interface iAdministradorService
    {
        Administrador? Login(LoginDTO loginDTO);
    }
}