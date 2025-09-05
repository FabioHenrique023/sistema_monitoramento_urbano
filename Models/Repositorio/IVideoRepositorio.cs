using sistema_monitoramento_urbano.Models.Repositorio.Entidades;

namespace sistema_monitoramento_urbano.Models.Repositorio
{

    public interface IVideoRepositorio : IRepositorio<Video>
    {
        IEnumerable<Video> BuscarPorCamera(int idCamera);
    }
}