function() {
    var fixtureContainer = karate.read('classpath:com/mnemotoad/common/fixture-container.js');

    return fixtureContainer({
        create: function(overrides) {
            return karate.call('classpath:com/mnemotoad/knowledgenode/create-knowledgenode.feature', overrides || {});
        },
        remove: function(knowledgeNodeId) {
            karate.call('classpath:com/mnemotoad/knowledgenode/delete-knowledgenode.feature', { knowledgeNodeId: knowledgeNodeId });
        }
    });
}
