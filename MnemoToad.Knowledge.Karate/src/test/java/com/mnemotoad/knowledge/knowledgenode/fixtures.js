function() {
    var fixtureContainer = karate.read('classpath:com/mnemotoad/knowledge/common/fixture-container.js');

    return fixtureContainer({
        create: function(overrides) {
            return karate.call('classpath:com/mnemotoad/knowledge/knowledgenode/create-knowledgenode.feature', overrides || {});
        },
        remove: function(knowledgeNodeId) {
            karate.call('classpath:com/mnemotoad/knowledge/knowledgenode/delete-knowledgenode.feature', { knowledgeNodeId: knowledgeNodeId });
        }
    });
}
