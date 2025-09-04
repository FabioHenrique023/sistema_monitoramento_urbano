using System.Data;
using Dapper;
using InfraEstrutura;

namespace Models.Repositorio.Entidades
{
    public class CameraRepositorio : IRepositorio<Camera>
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
                FROM Cameras
                ORDER BY Id DESC;";
            return _dbConnection.Query<Camera>(sql);
        }

        public Camera Buscar(int id) 
        {
            const string sql = @"
                SELECT Id, Descricao, Latitude, Longitude, Fps
                FROM Cameras
                WHERE Id = @Id;";
            var result = _dbConnection.QuerySingleOrDefault<Camera>(sql, new { Id = id });

            if (result is null)
                throw new KeyNotFoundException($"Camera Id {id} não encontrada.");
            return result;
        }

        public void Inserir(Camera model)
        {
            const string sql = @"
                INSERT INTO Cameras (Descricao, Latitude, Longitude, Fps)
                VALUES (@Descricao, @Latitude, @Longitude, @Fps);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var newId = _dbConnection.ExecuteScalar<int>(sql, new
            {
                model.Descricao,
                model.Latitude,
                model.Longitude,
                model.Fps
            });

            model.Id = newId;
        }

        public void Alterar(Camera model)
        {
            const string sql = @"
                UPDATE Cameras
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
            const string sql = "DELETE FROM Cameras WHERE Id = @Id;";
            _dbConnection.Execute(sql, new { Id = id });
        }
    }
}