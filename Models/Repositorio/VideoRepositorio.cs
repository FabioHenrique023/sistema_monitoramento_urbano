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
                SELECT id, nome_arquivo, caminho_arquivo, data_upload, horario_inicio, id_usuario, camera_id
                FROM public.video
                ORDER BY created_at DESC;";
            return _dbConnection.Query<Video>(sql);
        }

        public IEnumerable<Video> BuscarPorCamera(int idCamera)
        {
            const string sql = @"
                SELECT id, nome_arquivo, caminho_arquivo, data_upload, horario_inicio, id_usuario, camera_id
                FROM public.video
                WHERE camera_id = @idCamera
                ORDER BY created_at DESC;";
            return _dbConnection.Query<Video>(sql, new { idCamera });
        }

        public Video Buscar(int id)
        {
            const string sql = @"
                SELECT id, nome_arquivo, caminho_arquivo, data_upload, horario_inicio, id_usuario, camera_id
                FROM public.video
                WHERE id = @id;";
            var result = _dbConnection.QuerySingleOrDefault<Video>(sql, new { id });

            if (result is null)
                throw new KeyNotFoundException($"Vídeo com Id {id} não encontrado.");
            return result;
        }

        public int Inserir(Video model)
        {
            const string sql = @"
                INSERT INTO public.video
                    (nome_arquivo, caminho_arquivo, data_upload, horario_inicio, id_usuario, camera_id)
                VALUES
                    (@nome_arquivo, @caminho_arquivo, @data_upload, @horario_inicio, @id_usuario, @camera_id)
                RETURNING id;";

            var newId = _dbConnection.ExecuteScalar<int>(sql, new
            {
                model.nome_arquivo,
                model.caminho_arquivo,
                model.data_upload,
                model.horario_inicio,
                model.id_usuario,
                model.camera_id
            });

            model.Id = newId;

            return newId;
        }

        public void Alterar(Video model)
        {
            const string sql = @"
                UPDATE public.video
                SET nome_arquivo   = @nome_arquivo,
                    caminho_arquivo = @caminho_arquivo,
                    data_upload    = @data_upload,
                    horario_inicio = @horario_inicio,
                    id_usuario     = @id_usuario,
                    camera_id      = @camera_id,
                    updated_at     = NOW()
                WHERE id = @Id;";

            _dbConnection.Execute(sql, new
            {
                model.nome_arquivo,
                model.caminho_arquivo,
                model.data_upload,
                model.horario_inicio,
                model.id_usuario,
                model.camera_id,
                model.Id
            });
        }

        public void Excluir(int id)
        {
            const string sql = "DELETE FROM public.video WHERE id = @id;";
            _dbConnection.Execute(sql, new { id });
        }
    }
}
