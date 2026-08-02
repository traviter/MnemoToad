function() {
    var fixtureContainer = karate.read('classpath:com/mnemotoad/common/fixture-container.js');

    return fixtureContainer({
        create: function(overrides) {
            return karate.call('classpath:com/mnemotoad/relationshiptype/create-relationshiptype.feature', overrides || {});
        },
        remove: function(relationshipTypeId) {
            karate.call('classpath:com/mnemotoad/relationshiptype/delete-relationshiptype.feature', { relationshipTypeId: relationshipTypeId });
        }
    });
}
