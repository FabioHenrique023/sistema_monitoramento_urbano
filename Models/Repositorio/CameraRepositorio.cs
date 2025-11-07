using System.Data;
using Dapper;
using InfraEstrutura;
using sistema_monitoramento_urbano.Models.Repositorio;

namespace sistema_monitoramento_urbano.Models.Repositorio.Entidades
{
    public class CameraRepositorio : ICameraRepositorio
    {
        private readonly IDbConnection _dbConnection;

        public CameraRepositorio(ISqlConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.CreateConnection();
        }

        public IEnumerable<Camera> BuscarTodos()
        {
            const string sql = @"
                SELECT Id, Descricao, Latitude, Longitude, Fps
                FROM cameras
                ORDER BY Id DESC;";
            return _dbConnection.Query<Camera>(sql);
        }

        public Camera Buscar(int id) 
        {
            const string sql = @"
                SELECT Id, Descricao, Latitude, Longitude, Fps
                FROM public.cameras
                WHERE Id = @Id;";
            var result = _dbConnection.QuerySingleOrDefault<Camera>(sql, new { Id = id });

            if (result is null)
                throw new KeyNotFoundException($"Camera Id {id} não encontrada.");
            return result;
        }

        public int Inserir(Camera model)
        {
            const string sql = @"
                INSERT INTO cameras (Descricao, Latitude, Longitude, Fps)
                VALUES (@Descricao, @Latitude, @Longitude, @Fps)
                RETURNING Id;";  // PostgreSQL retorna o Id da linha inserida

            var newId = _dbConnection.ExecuteScalar<int>(sql, new
            {
                model.Descricao,
                model.Latitude,
                model.Longitude,
                model.Fps
            });

            model.Id = newId;
            return newId;
        }

        public void Alterar(Camera model)
        {
            const string sql = @"
                UPDATE cameras
                SET Descricao = @Descricao,
                    Latitude  = @Latitude,
                    Longitude = @Longitude,
                    Fps       = @Fps
                WHERE Id = @Id;";

            _dbConnection.Execute(sql, new
            {
                model.Descricao,
                model.Latitude,
                model.Longitude,
                model.Fps,
                model.Id
            });
        }

        public void Excluir(int id)
        {
            const string sql = "DELETE FROM cameras WHERE Id = @Id;";
            _dbConnection.Execute(sql, new { Id = id });
        }
    }
}