using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace sistema_monitoramento_urbano.Models.Repositorio.Entidades
{
    public class Camera
    {
        public virtual int Id { get; set; }

        [Required, MaxLength(200)]
        public virtual string Descricao { get; set; }

        [Required, Range(-90, 90)]
        public virtual double Latitude { get; set; }

        [Required, Range(-180, 180)]
        public virtual double Longitude { get; set; }

        [Required, MaxLength(50)]
        public virtual string Fps { get; set; }

        // Construtor para garantir que todos os campos obrigatórios sejam preenchidos
        [SetsRequiredMembers]
        public Camera(string descricao, double latitude, double longitude, string fps)
        {
            Descricao = descricao;
            Latitude  = latitude;
            Longitude = longitude;
            Fps       = fps;
        }

        // Necessário para o EF Core
        protected Camera() { }
    }
}
