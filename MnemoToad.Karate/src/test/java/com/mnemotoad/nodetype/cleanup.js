function() {
    var ids = karate.get('createdNodeTypeIds');
    if (ids) {
        for (var i = 0; i < ids.length; i++) {
            try {
                karate.call('classpath:com/mnemotoad/nodetype/delete-nodetype.feature', { nodeTypeId: ids[i] });
            } catch (e) {
                karate.log('node type cleanup failed (suppressed):', e);
            }
        }
    }
}