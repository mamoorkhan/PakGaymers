using Microsoft.Azure.Cosmos.Table;
using PakGaymers.Data.Table.Models;
using System.Threading.Tasks;

namespace PakGaymers.Data.Table
{
    public interface ITableStorage
    {
        public Task<CloudTable> CreateTableAsync(string tableName);

        public Task DeleteEntityAsync(CloudTable table, UserEntity deleteEntity);

        public Task<UserEntity> RetrieveEntityUsingPointQueryAsync(CloudTable table, string partitionKey, string rowKey);

        public Task<UserEntity> InsertOrMergeEntityAsync(CloudTable table, UserEntity entity);
    }
}