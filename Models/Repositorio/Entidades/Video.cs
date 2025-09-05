using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace sistema_monitoramento_urbano.Models.Repositorio.Entidades
{
    public class Video
    {
        public virtual int Id { get; set; }

        [Required, MaxLength(200)]
        public virtual string nome_arquivo { get; set; }

        [Required, MaxLength(500)]
        public virtual string caminho_arquivo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public virtual string data_upload { get; set; } // Ex: "21/08/2025"

        [Required]
        [DataType(DataType.Time)]
        public virtual string horario_inicio { get; set; } // Ex: "14:15"

        [Required]
        public virtual int id_usuario { get; set; }

        [Required]
        public virtual int camera_id { get; set; }

        // Construtor para garantir que todos os campos obrigatórios sejam preenchidos
        [SetsRequiredMembers]
        public Video(string nome_arquivo, string caminho_arquivo, string data_upload, string horario_inicio, int id_usuario, int camera_id)
        {
            nome_arquivo    = nome_arquivo;
            caminho_arquivo = caminho_arquivo;
            data_upload     = data_upload;
            horario_inicio  = horario_inicio;
            id_usuario      = id_usuario;
            camera_id       = camera_id;
        }

        // Necessário para o EF Core
        public Video() { }
    }
}