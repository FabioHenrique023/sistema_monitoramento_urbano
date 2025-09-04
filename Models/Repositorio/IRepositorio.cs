namespace Models.Repositorio.Entidades;
using System.Collections.Generic;

public interface IRepositorio<TEntidade>
{
    IEnumerable<TEntidade> BuscarTodos();
    void Inserir(TEntidade model);
    void Alterar(TEntidade model);
    void Excluir(int Id);
    TEntidade Buscar(int Id);
}