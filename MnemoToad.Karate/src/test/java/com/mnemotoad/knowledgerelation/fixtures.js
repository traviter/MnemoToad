function() {
    var fixtureContainer = karate.read('classpath:com/mnemotoad/common/fixture-container.js');

    return fixtureContainer({
        create: function(overrides) {
            return karate.call('classpath:com/mnemotoad/knowledgerelation/create-knowledgerelation.feature', overrides || {});
        },
        remove: function(knowledgeRelationId) {
            karate.call('classpath:com/mnemotoad/knowledgerelation/delete-knowledgerelation.feature', { knowledgeRelationId: knowledgeRelationId });
        }
    });
}
