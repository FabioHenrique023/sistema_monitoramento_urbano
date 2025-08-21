using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Models.Repositorio.Entidades
{
    public class Cameras
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public required string Descricao { get; set; }

        [Required, Range(-90, 90)]
        public double Latitude { get; set; }

        [Required, Range(-180, 180)]
        public double Longitude { get; set; }

        [Required, MaxLength(50)]
        public required string Fps { get; set; }

        // Construtor para garantir que todos os campos obrigatórios sejam preenchidos
        [SetsRequiredMembers]
        public Cameras(string descricao, double latitude, double longitude, string fps)
        {
            Descricao = descricao;
            Latitude  = latitude;
            Longitude = longitude;
            Fps       = fps;
        }

        // Necessário para o EF Core
        protected Cameras() { }
    }
}
