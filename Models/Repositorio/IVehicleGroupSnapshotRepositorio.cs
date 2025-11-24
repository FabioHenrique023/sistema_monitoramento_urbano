using sistema_monitoramento_urbano.Models.Repositorio.Entidades;

namespace sistema_monitoramento_urbano.Models.Repositorio
{
    public interface IVehicleGroupSnapshotRepositorio : IRepositorio<VehicleGroupSnapshot>
    {
        VehicleGroupSnapshot? BuscarMaisRecente();
    }
}


