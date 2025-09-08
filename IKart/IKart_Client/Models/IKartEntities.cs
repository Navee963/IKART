using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace IKart_ServerSide.Models
{
    public class IKartEntities : DbContext
    {
        public IKartEntities() : base("name=IKartEntities") // Matches connection string name in web.config
        {
        }

        public DbSet<EmiCard_Documents> EmiCard_Documents { get; set; }
        // Add other DbSets as needed
    }
    public class EmiCard_Documents
    {
        [Key]
        public int DocumentId { get; set; }
        public int Card_Id { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedDate { get; set; }
    }

}