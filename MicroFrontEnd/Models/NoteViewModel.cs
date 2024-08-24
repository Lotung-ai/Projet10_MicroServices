using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MicroFrontEnd.Models
{
    public class NoteViewModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int PatId { get; set; }
        public string Patient { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
