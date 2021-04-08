using Microsoft.Azure.Cosmos.Table;

namespace PakGaymers.Data.Table.Models
{
    public class UserEntity : TableEntity
    {
        public UserEntity(string lastName, string firstName)
        {
            PartitionKey = lastName;
            RowKey = firstName;
        }

        public UserEntity()
        {
        }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }
}