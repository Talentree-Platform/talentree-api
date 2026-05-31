namespace Talentree.Core.Entities
{
    public class ArticleBookmark : AuditableEntity
    {
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public int ArticleId { get; set; }
        public KnowledgeArticle Article { get; set; } = null!;
    }
}