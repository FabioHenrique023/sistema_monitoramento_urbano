using System;

namespace sistema_monitoramento_urbano.Models.Repositorio.Entidades
{
    public class FrameProcessado
    {
        public int id { get; set; }
        public string caminho_imagem { get; set; } = string.Empty;
        public string? minutagem { get; set; }
        public string placa_detectada { get; set; } = string.Empty;
        public int? videos_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
