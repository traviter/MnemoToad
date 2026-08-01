function() {
    var knowledgeNodeIds = karate.get('createdKnowledgeNodeIds');
    if (knowledgeNodeIds) {
        for (var i = 0; i < knowledgeNodeIds.length; i++) {
            try {
                karate.call('classpath:com/mnemotoad/knowledgenode/delete-knowledgenode.feature', { knowledgeNodeId: knowledgeNodeIds[i] });
            } catch (e) {
                karate.log('knowledge node cleanup failed (suppressed):', e);
            }
        }
    }

    // KnowledgeNodes must be deleted before their referenced NodeTypes -- the FK on
    // knowledge_node.node_type_id would otherwise reject the NodeType delete.
    var nodeTypeIds = karate.get('createdNodeTypeIds');
    if (nodeTypeIds) {
        for (var i = 0; i < nodeTypeIds.length; i++) {
            try {
                karate.call('classpath:com/mnemotoad/nodetype/delete-nodetype.feature', { nodeTypeId: nodeTypeIds[i] });
            } catch (e) {
                karate.log('node type cleanup failed (suppressed):', e);
            }
        }
    }
}
