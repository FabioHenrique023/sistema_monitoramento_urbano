using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using System.Collections.Generic;

namespace sistema_monitoramento_urbano.Models.Repositorio
{
    public interface IRepositorio<TEntidade>
    {
        IEnumerable<TEntidade> BuscarTodos();
        void Inserir(TEntidade model);
        void Alterar(TEntidade model);
        void Excluir(int Id);
        TEntidade Buscar(int Id);
    }
}