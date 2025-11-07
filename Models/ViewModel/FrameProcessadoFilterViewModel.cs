using System;

namespace sistema_monitoramento_urbano.Models.ViewModel
{
    public class FrameProcessadoFilterViewModel
    {
        public string? Placa { get; set; }
        public int? VideoId { get; set; }
        public DateTime? De { get; set; }
        public DateTime? Ate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
