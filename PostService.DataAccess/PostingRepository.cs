using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using PostService.CommonTypes;
using PostService.Models;

namespace PostService.DataAccess;

public class PostingRepository : IPostingRepository
{
    private const string ConnectionString = "Data Source=PostingDb.db;";

    public void CreateDb()
    {
        CreatePostingTable();
    }

    private void CreatePostingTable()
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sqlText = @"
            CREATE TABLE IF NOT EXISTS Postings (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [From] TEXT NOT NULL,
                [To] TEXT NOT NULL,
                [Content] TEXT NOT NULL,
                DeliveryType INTEGER NOT NULL,
                Weight REAL NOT NULL,
                Width REAL NOT NULL,
                Height REAL NOT NULL,
                Depth REAL NOT NULL,
                [Value] REAL NULL,
                [Price] REAL NOT NULL,
                [CreatedAt] INTEGER NOT NULL
            );";

        using var cmd = new SqliteCommand(sqlText, conn);
        cmd.ExecuteNonQuery();
    }

    public List<Posting> GetList()
    {
        var resultList = new List<Posting>();

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sqlText = @"
            SELECT Id, [From], [To], [Content], DeliveryType,
                   Weight, Width, Height, Depth, [Value], [Price], [CreatedAt]
            FROM Postings;";

        using var cmd = new SqliteCommand(sqlText, conn);
        using var reader = cmd.ExecuteReader();

        var idIndex = reader.GetOrdinal("Id");
        var fromIndex = reader.GetOrdinal("From");
        var toIndex = reader.GetOrdinal("To");
        var contentIndex = reader.GetOrdinal("Content");
        var deliveryTypeIndex = reader.GetOrdinal("DeliveryType");
        var weightIndex = reader.GetOrdinal("Weight");
        var widthIndex = reader.GetOrdinal("Width");
        var heightIndex = reader.GetOrdinal("Height");
        var depthIndex = reader.GetOrdinal("Depth");
        var valueIndex = reader.GetOrdinal("Value");
        var priceIndex = reader.GetOrdinal("Price");
        var createdAtIndex = reader.GetOrdinal("CreatedAt");

        while (reader.Read())
        {
            var posting = new Posting
            {
                Id = reader.GetInt32(idIndex),
                From = reader.GetString(fromIndex),
                To = reader.GetString(toIndex),
                Content = reader.GetString(contentIndex),
                DeliveryType = (DeliveryType)reader.GetInt32(deliveryTypeIndex),
                Weight = reader.GetFloat(weightIndex),
                Width = reader.GetFloat(widthIndex),
                Height = reader.GetFloat(heightIndex),
                Depth = reader.GetFloat(depthIndex),
                Value = reader.IsDBNull(valueIndex) ? null : reader.GetFloat(valueIndex),
                Price = reader.GetFloat(priceIndex),
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(createdAtIndex)).UtcDateTime
            };

            resultList.Add(posting);
        }

        return resultList;
    }

    public Posting? GetById(int postingId)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sqlText = @"
            SELECT Id, [From], [To], [Content], DeliveryType,
                   Weight, Width, Height, Depth, [Value], [Price], [CreatedAt]
            FROM Postings
            WHERE Id = @PostingId;";

        using var cmd = new SqliteCommand(sqlText, conn);
        cmd.Parameters.AddWithValue("@PostingId", postingId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var idIndex = reader.GetOrdinal("Id");
        var fromIndex = reader.GetOrdinal("From");
        var toIndex = reader.GetOrdinal("To");
        var contentIndex = reader.GetOrdinal("Content");
        var deliveryTypeIndex = reader.GetOrdinal("DeliveryType");
        var weightIndex = reader.GetOrdinal("Weight");
        var widthIndex = reader.GetOrdinal("Width");
        var heightIndex = reader.GetOrdinal("Height");
        var depthIndex = reader.GetOrdinal("Depth");
        var valueIndex = reader.GetOrdinal("Value");
        var priceIndex = reader.GetOrdinal("Price");
        var createdAtIndex = reader.GetOrdinal("CreatedAt");

        var posting = new Posting
        {
            Id = reader.GetInt32(idIndex),
            From = reader.GetString(fromIndex),
            To = reader.GetString(toIndex),
            Content = reader.GetString(contentIndex),
            DeliveryType = (DeliveryType)reader.GetInt32(deliveryTypeIndex),
            Weight = reader.GetFloat(weightIndex),
            Width = reader.GetFloat(widthIndex),
            Height = reader.GetFloat(heightIndex),
            Depth = reader.GetFloat(depthIndex),
            Value = reader.IsDBNull(valueIndex) ? null : reader.GetFloat(valueIndex),
            Price = reader.GetFloat(priceIndex),
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(createdAtIndex)).UtcDateTime
        };

        return posting;
    }

    public int Create(Posting posting)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sqlText = @"
            INSERT INTO Postings ([From], [To], [Content], DeliveryType,
                                  Weight, Width, Height, Depth,
                                  [Value], [Price], [CreatedAt])
            VALUES (@From, @To, @Content, @DeliveryType,
                    @Weight, @Width, @Height, @Depth,
                    @Value, @Price, @CreatedAt)
            RETURNING Id;";

        using var cmd = new SqliteCommand(sqlText, conn);
        cmd.Parameters.AddWithValue("@From", posting.From);
        cmd.Parameters.AddWithValue("@To", posting.To);
        cmd.Parameters.AddWithValue("@Content", posting.Content);
        cmd.Parameters.AddWithValue("@DeliveryType", (int)posting.DeliveryType);
        cmd.Parameters.AddWithValue("@Weight", posting.Weight);
        cmd.Parameters.AddWithValue("@Width", posting.Width);
        cmd.Parameters.AddWithValue("@Height", posting.Height);
        cmd.Parameters.AddWithValue("@Depth", posting.Depth);
        cmd.Parameters.AddWithValue("@Value", posting.Value ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Price", posting.Price);
        cmd.Parameters.AddWithValue("@CreatedAt", new DateTimeOffset(posting.CreatedAt).ToUnixTimeSeconds());

        var generatedId = Convert.ToInt32(cmd.ExecuteScalar());
        return generatedId;
    }

    public int Update(Posting posting)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sqlText = @"
            UPDATE Postings
            SET [From] = @From,
                [To] = @To,
                [Content] = @Content,
                DeliveryType = @DeliveryType,
                Weight = @Weight,
                Width = @Width,
                Height = @Height,
                Depth = @Depth,
                [Value] = @Value,
                [Price] = @Price
            WHERE Id = @Id;";

        using var cmd = new SqliteCommand(sqlText, conn);
        cmd.Parameters.AddWithValue("@Id", posting.Id);
        cmd.Parameters.AddWithValue("@From", posting.From);
        cmd.Parameters.AddWithValue("@To", posting.To);
        cmd.Parameters.AddWithValue("@Content", posting.Content);
        cmd.Parameters.AddWithValue("@DeliveryType", (int)posting.DeliveryType);
        cmd.Parameters.AddWithValue("@Weight", posting.Weight);
        cmd.Parameters.AddWithValue("@Width", posting.Width);
        cmd.Parameters.AddWithValue("@Height", posting.Height);
        cmd.Parameters.AddWithValue("@Depth", posting.Depth);
        cmd.Parameters.AddWithValue("@Value", posting.Value ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Price", posting.Price);

        var rowsAffected = cmd.ExecuteNonQuery();
        return rowsAffected;
    }

    public int Delete(int postingId)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sqlText = "DELETE FROM Postings WHERE Id = @PostingId;";
        using var cmd = new SqliteCommand(sqlText, conn);
        cmd.Parameters.AddWithValue("@PostingId", postingId);

        var numberOfDeletedRecords = cmd.ExecuteNonQuery();
        return numberOfDeletedRecords;
    }
}