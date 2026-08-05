using MnemoToad.Knowledge.Data;
using MnemoToad.Knowledge.Data.Entities;

namespace MnemoToad.Knowledge.Tests.TestSupport;

internal static class DbFixtures
{
    public static async Task<NodeType> CreateNodeTypeAsync(this IAppDbContext db, string? name = null, string? description = null)
    {
        var nodeType = new NodeType { Name = name ?? $"NodeType_{Guid.NewGuid()}", Description = description };
        db.NodeType.Add(nodeType);
        await db.SaveChangesAsync();
        return nodeType;
    }

    public static async Task<KnowledgeNode> CreateKnowledgeNodeAsync(this IAppDbContext db, Guid nodeTypeId, string? canonicalName = null, string? description = null)
    {
        var knowledgeNode = new KnowledgeNode
        {
            NodeTypeId = nodeTypeId,
            CanonicalName = canonicalName ?? $"KnowledgeNode_{Guid.NewGuid()}",
            Description = description
        };
        db.KnowledgeNode.Add(knowledgeNode);
        await db.SaveChangesAsync();
        return knowledgeNode;
    }

    public static async Task<RelationshipType> CreateRelationshipTypeAsync(this IAppDbContext db, string? name = null, string? inverseName = null, string? description = null)
    {
        var relationshipType = new RelationshipType
        {
            Name = name ?? $"RelationshipType_{Guid.NewGuid()}",
            InverseName = inverseName,
            Description = description
        };
        db.RelationshipType.Add(relationshipType);
        await db.SaveChangesAsync();
        return relationshipType;
    }

    public static async Task<KnowledgeRelation> CreateKnowledgeRelationAsync(this IAppDbContext db, Guid sourceNodeId, Guid relationshipTypeId, Guid targetNodeId)
    {
        var knowledgeRelation = new KnowledgeRelation
        {
            SourceNodeId = sourceNodeId,
            RelationshipTypeId = relationshipTypeId,
            TargetNodeId = targetNodeId
        };
        db.KnowledgeRelation.Add(knowledgeRelation);
        await db.SaveChangesAsync();
        return knowledgeRelation;
    }

    public static async Task<AttributeType> CreateAttributeTypeAsync(this IAppDbContext db, string? name = null, string? description = null)
    {
        var attributeType = new AttributeType { Name = name ?? $"AttributeType_{Guid.NewGuid()}", Description = description };
        db.AttributeType.Add(attributeType);
        await db.SaveChangesAsync();
        return attributeType;
    }

    public static async Task<KnowledgeNodeAttribute> CreateKnowledgeNodeAttributeAsync(this IAppDbContext db, Guid knowledgeNodeId, Guid attributeTypeId, string? value = null)
    {
        var knowledgeNodeAttribute = new KnowledgeNodeAttribute
        {
            KnowledgeNodeId = knowledgeNodeId,
            AttributeTypeId = attributeTypeId,
            Value = value ?? $"Value_{Guid.NewGuid()}"
        };
        db.KnowledgeNodeAttribute.Add(knowledgeNodeAttribute);
        await db.SaveChangesAsync();
        return knowledgeNodeAttribute;
    }
}
