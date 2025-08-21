using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Models.Repositorio.Entidades
{
    public class Video
    {
        public virtual int Id { get; set; }

        [Required, MaxLength(200)]
        public virtual required string NomeArquivo { get; set; }

        [Required, MaxLength(500)]
        public virtual required string CaminhoArquivo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public virtual required string DataUpload { get; set; } // Ex: "21/08/2025"

        [Required]
        [DataType(DataType.Time)]
        public virtual required string HorarioInicio { get; set; } // Ex: "14:15"

        [Required]
        public virtual int IdUsuario { get; set; }

        [Required]
        public virtual int CameraId { get; set; }

        // Construtor para garantir que todos os campos obrigatórios sejam preenchidos
        [SetsRequiredMembers]
        public Video(string nomeArquivo, string caminhoArquivo, string dataUpload, string horarioInicio, int idUsuario, int cameraId)
        {
            NomeArquivo    = nomeArquivo;
            CaminhoArquivo = caminhoArquivo;
            DataUpload     = dataUpload;
            HorarioInicio  = horarioInicio;
            IdUsuario      = idUsuario;
            CameraId       = cameraId;
        }

        // Necessário para o EF Core
        public Video() { }
    }
}