using System.Data;
using Dapper;
using Models.Repositorio.Entidades;

namespace Models.Repositorio.Entidades
{
    public class VideoRepositorio : IRepositorio<Video>
    {
        private readonly IDbConnection _dbConnection;

        public VideoRepositorio(ISqlConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.CreateConnection();
        }

        public IEnumerable<Video> BuscarTodos()
        {
            const string sql = @"
                SELECT Id, NomeArquivo, CaminhoArquivo, DataUpload, HorarioInicio, IdUsuario, CameraId
                FROM Videos
                ORDER BY Id DESC;";
            return _dbConnection.Query<Video>(sql);
        }

        public IEnumerable<Video> BuscarPorCamera(int idCamera)
        {
            return _dbConnection.Query<Video>("SELECT * FROM Cameras WHERE Id = @IdCamera", new { idCamera = idCamera });
        }

        public Video Buscar(int id)
        {
            const string sql = @"
                    SELECT Id, NomeArquivo, CaminhoArquivo, DataUpload, HorarioInicio, IdUsuario, CameraId
                    FROM Videos
                    WHERE Id = @Id;";
            var result = _dbConnection.QuerySingleOrDefault<Video>(sql, new { Id = id });

            if (result is null)
                throw new KeyNotFoundException($"Video com Id {id} não encontrado.");
            return result;
        }

        public void Inserir(Video model)
        {
            const string sql = @"
                    INSERT INTO Videos (NomeArquivo, CaminhoArquivo, DataUpload, HorarioInicio, IdUsuario, CameraId)
                    VALUES (@NomeArquivo, @CaminhoArquivo, @DataUpload, @HorarioInicio, @IdUsuario, @CameraId);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var newId = _dbConnection.ExecuteScalar<int>(sql, new
            {
                model.NomeArquivo,
                model.CaminhoArquivo,
                model.DataUpload,    // string "dd/MM/yyyy" (validar no Controller/ViewModel)
                model.HorarioInicio, // string "HH:mm" (validar no Controller/ViewModel)
                model.IdUsuario,
                model.CameraId
            });

            model.Id = newId;
        }

        public void Alterar(Video model)
        {
            const string sql = @"
                UPDATE Videos
                SET NomeArquivo   = @NomeArquivo,
                    CaminhoArquivo= @CaminhoArquivo,
                    DataUpload    = @DataUpload,
                    HorarioInicio = @HorarioInicio,
                    IdUsuario     = @IdUsuario,
                    CameraId      = @CameraId
                WHERE Id = @Id;";

            _dbConnection.Execute(sql, new
            {
                model.NomeArquivo,
                model.CaminhoArquivo,
                model.DataUpload,
                model.HorarioInicio,
                model.IdUsuario,
                model.CameraId,
                model.Id
            });
        }

        public void Excluir(int id)
        {
            const string sql = "DELETE FROM Videos WHERE Id = @Id;";
            _dbConnection.Execute(sql, new { Id = id });
        }
    }
}