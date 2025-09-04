using System.ComponentModel.DataAnnotations;
using Models.Repositorio.Entidades;

namespace Models.ViewModel
{
    public class VideosViewModel : Video
    {
        public override int Id { get; set; }

        [Required(ErrorMessage = "O nome do arquivo é obrigatório.")]
        [StringLength(200, ErrorMessage = "Máximo de 200 caracteres.")]
        public override string NomeArquivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O caminho/URL do arquivo é obrigatório.")]
        [StringLength(500, ErrorMessage = "Máximo de 500 caracteres.")]
        public override string CaminhoArquivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de upload é obrigatória.")]
        [RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "Use o formato dd/MM/yyyy (ex.: 21/08/2025).")]
        public override string DataUpload { get; set; } = string.Empty;

        [Required(ErrorMessage = "O horário de início é obrigatório.")]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Use o formato HH:mm (ex.: 14:15).")]
        public override string HorarioInicio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Usuário é obrigatório.")]
        public override int IdUsuario { get; set; }

        [Required(ErrorMessage = "A câmera vinculada é obrigatória.")]
        public override int CameraId { get; set; }

        public static VideosViewModel ViewModel(Video entidade)
        {
            if (entidade == null)
                throw new ArgumentNullException(nameof(entidade));

            Type tipoDestino = typeof(VideosViewModel);
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
            return (VideosViewModel)instancia!;
        }
    }
}