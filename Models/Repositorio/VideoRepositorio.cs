using System.Data;
using Dapper;
using InfraEstrutura;

namespace sistema_monitoramento_urbano.Models.Repositorio.Entidades
{
    public class VideoRepositorio : IVideoRepositorio
    {
        private readonly IDbConnection _dbConnection;

        public VideoRepositorio(ISqlConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.CreateConnection();
        }

        public IEnumerable<Video> BuscarTodos()
        {
            const string sql = @"
                SELECT Id, nome_arquivo, data_upload, horario_inicio, id_usuario, camera_id
                FROM Video
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
                    SELECT Id, nome_arquivo, data_upload, horario_inicio, id_usuario, camera_id
                    FROM Video
                    WHERE Id = @Id;";
            var result = _dbConnection.QuerySingleOrDefault<Video>(sql, new { Id = id });

            if (result is null)
                throw new KeyNotFoundException($"Video com Id {id} não encontrado.");
            return result;
        }

        public void Inserir(Video model)
        {
            const string sql = @"
                    INSERT INTO Video (nome_arquivo, data_upload, horario_inicio, id_usuario, camera_id)
                    VALUES (@nome_arquivo, @data_upload, @horario_inicio, @id_usuario, @camera_id);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var newId = _dbConnection.ExecuteScalar<int>(sql, new
            {
                model.nome_arquivo,
                model.data_upload,    // string "dd/MM/yyyy" (validar no Controller/ViewModel)
                model.horario_inicio, // string "HH:mm" (validar no Controller/ViewModel)
                model.id_usuario,
                model.camera_id
            });

            model.Id = newId;
        }

        public void Alterar(Video model)
        {
            const string sql = @"
                UPDATE Video
                SET nome_arquivo   = @nome_arquivo,
                    data_upload    = @data_upload,
                    horario_inicio = @horario_inicio,
                    id_usuario     = @id_usuario,
                    camera_id      = @camera_id
                WHERE Id = @Id;";

            _dbConnection.Execute(sql, new
            {
                model.nome_arquivo,
                model.data_upload,
                model.horario_inicio,
                model.id_usuario,
                model.camera_id,
                model.Id
            });
        }

        public void Excluir(int id)
        {
            const string sql = "DELETE FROM Video WHERE Id = @Id;";
            _dbConnection.Execute(sql, new { Id = id });
        }
    }
}