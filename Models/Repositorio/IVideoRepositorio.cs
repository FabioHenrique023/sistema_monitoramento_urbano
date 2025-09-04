using Models.Repositorio.Entidades;

namespace Models.Repositorio;

public interface IVideoRepositorio: IRepositorio<Video>
{
    IEnumerable<Video> BuscarPorCamera(int idCamera);
}