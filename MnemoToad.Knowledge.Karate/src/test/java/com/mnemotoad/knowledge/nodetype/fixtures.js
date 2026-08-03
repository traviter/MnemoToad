function() {
    var fixtureContainer = karate.read('classpath:com/mnemotoad/knowledge/common/fixture-container.js');

    return fixtureContainer({
        create: function(overrides) {
            return karate.call('classpath:com/mnemotoad/knowledge/nodetype/create-nodetype.feature', overrides || {});
        },
        remove: function(nodeTypeId) {
            karate.call('classpath:com/mnemotoad/knowledge/nodetype/delete-nodetype.feature', { nodeTypeId: nodeTypeId });
        }
    });
}
