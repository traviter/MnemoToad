function() {
    var fixtureContainer = karate.read('classpath:com/mnemotoad/knowledge/common/fixture-container.js');

    return fixtureContainer({
        create: function(overrides) {
            return karate.call('classpath:com/mnemotoad/knowledge/mediaasset/create-mediaasset.feature', overrides || {});
        },
        remove: function(mediaAssetId) {
            karate.call('classpath:com/mnemotoad/knowledge/mediaasset/delete-mediaasset.feature', { mediaAssetId: mediaAssetId });
        }
    });
}
