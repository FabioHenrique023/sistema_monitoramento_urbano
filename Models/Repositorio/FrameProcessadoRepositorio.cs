using System.Data;
using Dapper;
using InfraEstrutura;
using sistema_monitoramento_urbano.Models.Repositorio;

namespace sistema_monitoramento_urbano.Models.Repositorio.Entidades
{
    public class FrameProcessadoRepositorio : IFrameProcessadoRepositorio
    {
        private readonly IDbConnection _dbConnection;

        public FrameProcessadoRepositorio(ISqlConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.CreateConnection();
        }

        public IEnumerable<FrameProcessado> BuscarTodos()
        {
            const string sql = @"
                SELECT id, caminho_imagem, minutagem, created_at
                FROM frame_processados
                ORDER BY id DESC;";
            return _dbConnection.Query<FrameProcessado>(sql);
        }

        public FrameProcessado Buscar(int id) 
        {
            const string sql = @"
                SELECT id, caminho_imagem, minutagem, created_at
                FROM frame_processados
                WHERE id = @Id;";
            var result = _dbConnection.QuerySingleOrDefault<FrameProcessado>(sql, new { Id = id });

            if (result is null)
                throw new KeyNotFoundException($"Frame Processado Id {id} não encontrada.");
            return result;
        }

        public int Inserir(FrameProcessado model)
        {
            throw new NotImplementedException();
        }

        public void Alterar(FrameProcessado model)
        {
            throw new NotImplementedException();
        }

        public void Excluir(int Id)
        {
            throw new NotImplementedException();
        }
    }
}