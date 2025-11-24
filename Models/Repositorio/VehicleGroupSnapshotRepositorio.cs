using System.Collections.Generic;
using System.Data;
using Dapper;
using InfraEstrutura;

namespace sistema_monitoramento_urbano.Models.Repositorio.Entidades
{
    public class VehicleGroupSnapshotRepositorio : IVehicleGroupSnapshotRepositorio
    {
        private readonly IDbConnection _dbConnection;

        public VehicleGroupSnapshotRepositorio(ISqlConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.CreateConnection();
        }

        public IEnumerable<VehicleGroupSnapshot> BuscarTodos()
        {
            const string sql = @"
                SELECT id, blob_path AS BlobPath, container_name AS ContainerName,
                       conteudo_json AS ConteudoJson, criado_em AS CriadoEm
                FROM vehicle_group_snapshots
                ORDER BY criado_em DESC;";
            return _dbConnection.Query<VehicleGroupSnapshot>(sql);
        }

        public VehicleGroupSnapshot? BuscarMaisRecente()
        {
            const string sql = @"
                SELECT id, blob_path AS BlobPath, container_name AS ContainerName,
                       conteudo_json AS ConteudoJson, criado_em AS CriadoEm
                FROM vehicle_group_snapshots
                ORDER BY criado_em DESC
                LIMIT 1;";
            return _dbConnection.QueryFirstOrDefault<VehicleGroupSnapshot>(sql);
        }

        public VehicleGroupSnapshot Buscar(int id)
        {
            const string sql = @"
                SELECT id, blob_path AS BlobPath, container_name AS ContainerName,
                       conteudo_json AS ConteudoJson, criado_em AS CriadoEm
                FROM vehicle_group_snapshots
                WHERE id = @id;";
            var resultado = _dbConnection.QuerySingleOrDefault<VehicleGroupSnapshot>(sql, new { id });
            if (resultado is null)
                throw new KeyNotFoundException($"Snapshot {id} não encontrado.");
            return resultado;
        }

        public int Inserir(VehicleGroupSnapshot model)
        {
            const string sql = @"
                INSERT INTO vehicle_group_snapshots (blob_path, container_name, conteudo_json, criado_em)
                VALUES (@BlobPath, @ContainerName, @ConteudoJson, @CriadoEm)
                RETURNING id;";
            var novoId = _dbConnection.ExecuteScalar<int>(sql, new
            {
                model.BlobPath,
                model.ContainerName,
                model.ConteudoJson,
                model.CriadoEm
            });
            model.Id = novoId;
            return novoId;
        }

        public void Alterar(VehicleGroupSnapshot model)
        {
            const string sql = @"
                UPDATE vehicle_group_snapshots
                SET blob_path = @BlobPath,
                    container_name = @ContainerName,
                    conteudo_json = @ConteudoJson,
                    criado_em = @CriadoEm
                WHERE id = @Id;";
            _dbConnection.Execute(sql, new
            {
                model.BlobPath,
                model.ContainerName,
                model.ConteudoJson,
                model.CriadoEm,
                model.Id
            });
        }

        public void Excluir(int id)
        {
            const string sql = "DELETE FROM vehicle_group_snapshots WHERE id = @id;";
            _dbConnection.Execute(sql, new { id });
        }
    }
}

