using Domain.Entities;
using Domain.Interfaces;
using Infrastucture.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace Infrastucture.Repositories
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly IMongoCollection<ErrorLog> _errorLogs;

        public ErrorLogRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _errorLogs = database.GetCollection<ErrorLog>(mongoDBSettings.Value.CollectionName);
        }

        public async Task AddErrorLogAsync(ErrorLog errorLog)
        {
            await _errorLogs.InsertOneAsync(errorLog);
        }

        public async Task<IEnumerable<ErrorLog>> GetControlledErrorsAsync()
        {
            var filter = Builders<ErrorLog>.Filter.Eq(e => e.IsControlled, true);
            return await _errorLogs.Find(filter).ToListAsync();
        }

        public async Task UpdateErrorLogAsync(ErrorLog errorLog)
        {
            var filter = Builders<ErrorLog>.Filter.Eq(e => e.Id, errorLog.Id);
            await _errorLogs.ReplaceOneAsync(filter, errorLog);
        }

        public async Task DeleteErrorLogAsync(string id)
        {
            var filter = Builders<ErrorLog>.Filter.Eq(e => e.Id, id);
            await _errorLogs.DeleteOneAsync(filter);
        }
    }
}
