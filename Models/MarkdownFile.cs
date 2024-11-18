using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Markcons.Models
{
    public class MarkdownFile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
