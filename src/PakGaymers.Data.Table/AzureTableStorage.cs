using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UserEntity = PakGaymers.Data.Table.Models.UserEntity;

namespace PakGaymers.Data.Table
{
    public class AzureTableStorage : ITableStorage
    {
        private readonly ILogger<AzureTableStorage> _logger;
        private readonly CloudStorageAccount _storageAccount;

        public AzureTableStorage(
            IConfiguration configuration,
            ILogger<AzureTableStorage> logger)
        {
            _logger = logger;
            try
            {
                _storageAccount = CloudStorageAccount.Parse(configuration["Values:AzureWebJobsStorage"]);
            }
            catch (FormatException)
            {
                _logger.LogCritical("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                _logger.LogCritical("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }
        }

        public async Task<CloudTable> CreateTableAsync(string tableName)
        {
            // Create a table client for interacting with the table service
            var tableClient = _storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            _logger.LogInformation("Create a Table for the demo");

            // Create a table client for interacting with the table service
            var table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                _logger.LogInformation("Created Table named: {0}", tableName);
            }
            else
            {
                _logger.LogError("Table {0} already exists", tableName);
            }

            return table;
        }

        public async Task DeleteEntityAsync(CloudTable table, UserEntity deleteEntity)
        {
            try
            {
                if (deleteEntity == null)
                {
                    throw new ArgumentNullException(nameof(deleteEntity));
                }

                var deleteOperation = TableOperation.Delete(deleteEntity);
                var result = await table.ExecuteAsync(deleteOperation);

                if (result.RequestCharge.HasValue)
                {
                    _logger.LogInformation("Request Charge of Delete Operation: " + result.RequestCharge);
                }
            }
            catch (StorageException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<UserEntity> RetrieveEntityUsingPointQueryAsync(CloudTable table, string partitionKey, string rowKey)
        {
            try
            {
                var retrieveOperation = TableOperation.Retrieve<UserEntity>(partitionKey, rowKey);
                var result = await table.ExecuteAsync(retrieveOperation);
                var customer = result.Result as UserEntity;
                if (customer != null)
                {
                    _logger.LogInformation("\t{0}\t{1}\t{2}\t{3}", customer.PartitionKey, customer.RowKey, customer.Email, customer.PhoneNumber);
                }

                if (result.RequestCharge.HasValue)
                {
                    _logger.LogInformation("Request Charge of Retrieve Operation: " + result.RequestCharge);
                }

                return customer;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<UserEntity> InsertOrMergeEntityAsync(CloudTable table, UserEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                // Create the InsertOrReplace table operation
                var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                var result = await table.ExecuteAsync(insertOrMergeOperation);
                var insertedCustomer = result.Result as UserEntity;

                if (result.RequestCharge.HasValue)
                {
                    _logger.LogInformation("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}