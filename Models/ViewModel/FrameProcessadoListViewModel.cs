using sistema_monitoramento_urbano.Models.Repositorio.Entidades;

namespace sistema_monitoramento_urbano.Models.ViewModel
{
    public class FrameProcessadoListViewModel
    {
        public FrameProcessadoFilterViewModel Filtro { get; set; } = new();
        public PagedResult<FrameProcessado> Resultado { get; set; } = new();
    }
}
