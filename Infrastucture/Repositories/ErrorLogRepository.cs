using Domain.Entities;
using Domain.Interfaces;
using Infrastucture.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace Infrastucture.Repositories
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly IMongoCollection<FailedPurchase> _errorLogs;

        public ErrorLogRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var settings = mongoDBSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _errorLogs = database.GetCollection<FailedPurchase>(settings.CollectionName);
        }

        public async Task AddErrorLogAsync(FailedPurchase errorLog)
        {
            errorLog.Id = Guid.NewGuid(); // Asignar un ID único
            errorLog.CreatedAt = DateTime.UtcNow; // Establecer la fecha de creación
            await _errorLogs.InsertOneAsync(errorLog);
        }

        public async Task<IEnumerable<FailedPurchase>> GetControlledErrorsAsync(IEnumerable<string> controlledErrorMessages)
        {
            var filter = Builders<FailedPurchase>.Filter.In(e => e.ErrorMessage, controlledErrorMessages);
            return await _errorLogs.Find(filter).ToListAsync();
        }

        public async Task UpdateErrorLogAsync(FailedPurchase errorLog)
        {
            var filter = Builders<FailedPurchase>.Filter.Eq(e => e.Id, errorLog.Id);
            await _errorLogs.ReplaceOneAsync(filter, errorLog);
        }

        public async Task DeleteErrorLogAsync(Guid id)
        {
            var filter = Builders<FailedPurchase>.Filter.Eq(e => e.Id, id);
            await _errorLogs.DeleteOneAsync(filter);
        }

        public async Task<IEnumerable<FailedPurchase>> GetAllErrorLogsAsync()
        {
            return await _errorLogs.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<FailedPurchase>> GetNewErrorLogsAsync()
        {
            // Lógica para obtener errores nuevos, por ejemplo, errores que se agregaron en los últimos 10 minutos
            //var filter = Builders<FailedPurchase>.Filter.Gt(x => x.OccurredAt, DateTime.UtcNow.AddMinutes(-10)); // Filtrar errores recientes
            //return await _errorLogs.Find(filter).ToListAsync();
            return await Task.FromResult<IEnumerable<FailedPurchase>>(new List<FailedPurchase>());
        }
    }
}
