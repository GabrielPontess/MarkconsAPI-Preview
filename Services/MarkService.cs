using Markcons.Extensions;
using Markcons.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Markcons.Services
{
    public class MarkService
    {
        private readonly IMongoCollection<MarkdownFile> _collection;

        public MarkService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("Markcons");
            _collection = database.GetCollection<MarkdownFile>("MarkdownFiles");
        }

        public async Task<ErroOr<bool>> CreateAsync(MarkdownFile mark)
        {
            try
            {
                await _collection.InsertOneAsync(mark);
                return ErroOr<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ex.ToErroOrFailure<bool>("Markdown.Create");
            }
        }

        public async Task<ErroOr<List<MarkdownFile>>> GetAllAsync()
        {
            try
            {
                var response = await _collection.Find(doc => true).ToListAsync();
                return ErroOr<List<MarkdownFile>>.Success(response);
            }
            catch (Exception ex)
            {
                return ex.ToErroOrFailure<List<MarkdownFile>>("Markdown.GetAll");
            }
        }

        public async Task<ErroOr<MarkdownFile>> GetByIdAsync(string id)
        {
            try
            {
                var filtro = Builders<MarkdownFile>.Filter.Eq(p => p.Id, id);
                var response = await _collection.Find(filtro).FirstOrDefaultAsync();
                return ErroOr<MarkdownFile>.Success(response);
            }
            catch (Exception ex)
            {
                return ex.ToErroOrFailure<MarkdownFile>("Markdown.GetById");
            }
        }


        public async Task<ErroOr<List<MarkdownFile>>> GetByPrefixAsync(string content)
        {
            try
            {
                var filtro = Builders<MarkdownFile>.Filter.Or(
                             Builders<MarkdownFile>.Filter.Regex("Title", new BsonRegularExpression(content, "i")),
                             Builders<MarkdownFile>.Filter.Regex("Content", new BsonRegularExpression(content, "i")));
                var response = await _collection.Find(filtro).ToListAsync();
                return ErroOr<List<MarkdownFile>>.Success(response);
            }
            catch (Exception ex)
            {

                return ex.ToErroOrFailure<List<MarkdownFile>>("Markdown.GetByPrefix");
            }
        }

        public async Task<ErroOr<List<MarkdownFile>>> GetByTitleAsync(string title)
        {
            try
            {
                var filtro = Builders<MarkdownFile>.Filter.Regex("Title", new BsonRegularExpression(title, "i"));
                var response = await _collection.Find(filtro).ToListAsync();
                return ErroOr<List<MarkdownFile>>.Success(response);
            }
            catch (Exception ex)
            {

                return ex.ToErroOrFailure<List<MarkdownFile>>("Markdown.GetByTitle");
            }
        }

        public async Task<ErroOr<UpdateResult>> UpdateAsync(string id, MarkdownFile pessoaAtualizada)
        {
            try
            {
                var filtro = Builders<MarkdownFile>.Filter.Eq(p => p.Id, id);
                var update = Builders<MarkdownFile>.Update
                                                   .Set(p => p.Title, pessoaAtualizada.Title)
                                                   .Set(p => p.Content, pessoaAtualizada.Content);

                var response = await _collection.UpdateOneAsync(filtro, update);

                if (response.MatchedCount <= 0)
                    throw new Exception("Markdown não encontrado.");

                return ErroOr<UpdateResult>.Success(response);
            }
            catch (Exception ex)
            {
                return ex.ToErroOrFailure<UpdateResult>("Markdown.UpdateAsync");
            }
        }

        public async Task<ErroOr<DeleteResult>> DeleteAsync(string id)
        {
            try
            {
                var filtro = Builders<MarkdownFile>.Filter.Eq(p => p.Id, id);
                var response = await _collection.DeleteOneAsync(filtro);
                return ErroOr<DeleteResult>.Success(response);
            }
            catch (Exception ex)
            {
                return ex.ToErroOrFailure<DeleteResult>("Markdown.Delete");
            }
        }
    }
}
