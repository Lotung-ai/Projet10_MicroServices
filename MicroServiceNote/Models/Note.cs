using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MicroServiceNote.Models
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("patId")]
        public int PatId { get; set; }
        [BsonElement("patient")]
        public string Patient { get; set; } = string.Empty;
        [BsonElement("note")]
        public string NoteText { get; set; } = string.Empty;

    }
}