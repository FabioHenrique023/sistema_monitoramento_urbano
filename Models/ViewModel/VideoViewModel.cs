using System.ComponentModel.DataAnnotations;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;

namespace sistema_monitoramento_urbano.Models.ViewModel
{
    public class VideoViewModel : Video
    {
        public override int Id { get; set; }

        [Required(ErrorMessage = "O nome do arquivo é obrigatório.")]
        [StringLength(200, ErrorMessage = "Máximo de 200 caracteres.")]
        public override string nome_arquivo { get; set; } = string.Empty;

        // [Required(ErrorMessage = "O caminho/URL do arquivo é obrigatório.")]
        // [StringLength(500, ErrorMessage = "Máximo de 500 caracteres.")]
        public override string? caminho_arquivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de upload é obrigatória.")]
        // [RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "Use o formato dd/MM/yyyy (ex.: 21/08/2025).")]
        public override string data_upload { get; set; } = string.Empty;

        [Required(ErrorMessage = "O horário de início é obrigatório.")]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Use o formato HH:mm (ex.: 14:15).")]
        public override string horario_inicio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Usuário é obrigatório.")]
        public override int id_usuario { get; set; }

        [Required(ErrorMessage = "A câmera vinculada é obrigatória.")]
        public override int camera_id { get; set; }

        public static VideoViewModel ViewModel(Video entidade)
        {
            if (entidade == null)
                throw new ArgumentNullException(nameof(entidade));

            Type tipoDestino = typeof(VideoViewModel);
            Type tipoOrigem = typeof(Video);

            var instancia = Activator.CreateInstance(tipoDestino);
            foreach (var propDestino in tipoDestino.GetProperties())
            {
                if (propDestino.CanRead && propDestino.CanWrite)
                {
                    var propOrigem = tipoOrigem.GetProperty(propDestino.Name);
                    if (propOrigem != null)
                    {
                        var valor = propOrigem.GetValue(entidade);
                        propDestino.SetValue(instancia, valor);
                    }
                }
            }
            return (VideoViewModel)instancia!;
        }
    }
}