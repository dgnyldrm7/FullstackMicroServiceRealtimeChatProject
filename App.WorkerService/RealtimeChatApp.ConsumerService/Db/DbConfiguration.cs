using Microsoft.Data.SqlClient;
using RealtimeChatApp.ConsumerService.Models;

namespace RealtimeChatApp.ConsumerService.Db
{
    public class DbConfiguration : IDbConfiguration
    {
        private readonly IConfiguration _configuration;
        public DbConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SaveMessageToDatabaseAsync(MessageModel message)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[DB ❌] Connection string bulunamadı!");
                    Console.ResetColor();
                    return;
                }

                if (string.IsNullOrEmpty(message.SenderId) || string.IsNullOrEmpty(message.ReceiverId))
                {
                    Console.WriteLine("[DB ⚠️] SenderId veya ReceiverId null, kayıt yapılmadı.");
                    return;
                }


                const string sql = @"
                INSERT INTO Messages 
                (SenderId, ReceiverId, Content, SentAt, IsDeletedBySender, IsDeletedByReceiver, IsGroupMessage)
                VALUES 
                (@SenderId, @ReceiverId, @Content, @SentAt, 0, 0, 0)";

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();
                using var command = new SqlCommand(sql, connection, transaction);

                command.Parameters.AddWithValue("@SenderId", message.SenderId ?? (object)DBNull.Value);
                
                command.Parameters.AddWithValue("@ReceiverId", message.ReceiverId ?? (object)DBNull.Value);

                command.Parameters.AddWithValue("@Content", message.Content ?? (object)DBNull.Value);

                var sentAt = message.SentAt == default ? DateTime.UtcNow : message.SentAt;
                command.Parameters.AddWithValue("@SentAt", sentAt);

                var rows = await command.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(rows > 0
                    ? $"[💾] Mesaj DB'ye kaydedildi: {message.Content}"
                    : "[DB ⚠️] Kayıt yapılmadı (0 satır etkilendi)");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[DB ❌] Kayıt hatası: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
