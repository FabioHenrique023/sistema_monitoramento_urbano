using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_monitoramento_urbano.Models.Repositorio.Entidades
{
    public class VehicleGroupSnapshot
    {
        public int Id { get; set; }

        [Required]
        public string BlobPath { get; set; } = string.Empty;

        [Required]
        public string ContainerName { get; set; } = string.Empty;

        [Required]
        public string ConteudoJson { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; }
    }
}

