using System.ComponentModel.DataAnnotations;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;

namespace sistema_monitoramento_urbano.Models.ViewModel
{
    public class CameraViewModel : Camera
    {
        public override int Id { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(200, ErrorMessage = "Máximo de 200 caracteres.")]
        public override string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Latitude é obrigatória.")]
        [Range(-90, 90, ErrorMessage = "Latitude deve estar entre -90 e 90.")]
        public override double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude é obrigatória.")]
        [Range(-180, 180, ErrorMessage = "Longitude deve estar entre -180 e 180.")]
        public override double Longitude { get; set; }

        [Required(ErrorMessage = "FPS é obrigatório.")]
        [StringLength(50, ErrorMessage = "Máximo de 50 caracteres.")]
        public override string Fps { get; set; } = string.Empty;

        public override bool BoAtivo { get; set; } = true;

        public static CameraViewModel ViewModel(Camera entidade)
        {
            if (entidade == null)
                throw new ArgumentNullException(nameof(entidade));

            Type tipoDestino = typeof(CameraViewModel);
            Type tipoOrigem = typeof(Camera);

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
            return (CameraViewModel)instancia!;
        }
    }
}