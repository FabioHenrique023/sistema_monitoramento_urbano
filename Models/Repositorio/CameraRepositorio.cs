using System.Data;
using Dapper;
using Models.Repositorio.Entidades;

namespace Models.Repositorio.Entidades
{
    public class CameraRepositorio : IRepositorio<Camera>
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public CameraRepositorio(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Camera> BuscarTodos()
        {
            const string sql = @"SELECT Id, Descricao, Latitude, Longitude, Fps
                                 FROM Cameras
                                 ORDER BY Id DESC;";
            using var conn = _connectionFactory.CreateConnection();
            return conn.Query<Camera>(sql);
        }

        public Camera? Buscar(int id)
        {
            const string sql = @"SELECT Id, Descricao, Latitude, Longitude, Fps
                                 FROM Cameras
                                 WHERE Id = @Id;";
            using var conn = _connectionFactory.CreateConnection();
            return conn.QuerySingleOrDefault<Camera>(sql, new { Id = id });
        }

        public void Inserir(Camera model)
        {
            const string sql = @"
                                INSERT INTO Cameras (Descricao, Latitude, Longitude, Fps)
                                VALUES (@Descricao, @Latitude, @Longitude, @Fps);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _connectionFactory.CreateConnection();
            var newId = conn.ExecuteScalar<int>(sql, new
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
            using var conn = _connectionFactory.CreateConnection();
            conn.Execute(sql, new
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
            using var conn = _connectionFactory.CreateConnection();
            conn.Execute(sql, new { Id = id });
        }
    }
}