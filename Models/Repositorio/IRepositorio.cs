namespace Models.Repositorio.Entidades;

public interface IRepositorio<TEntidade>
{
    IEnumerable<TEntidade> BuscarTodos();
    void Inserir(TEntidade model);
    void Alterar(TEntidade model);
    void Excluir(int Id);
    TEntidade Buscar(int Id);
}