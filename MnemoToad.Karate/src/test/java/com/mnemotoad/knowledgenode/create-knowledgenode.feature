Feature: Create a KnowledgeNode (reusable setup helper)

  Background:
    * url baseUrl

  Scenario:
    * def uniqueName = read('classpath:com/mnemotoad/common/util.js')
    * def createdNodeType = karate.get('nodeTypeId') ? null : karate.call('classpath:com/mnemotoad/nodetype/create-nodetype.feature')
    * def nodeTypeId = karate.get('nodeTypeId') ? karate.get('nodeTypeId') : createdNodeType.response.id
    * def canonicalName = karate.get('canonicalName') ? karate.get('canonicalName') : uniqueName('KnowledgeNode')
    * def description = karate.get('description')
    Given path 'nodes'
    And request { nodeTypeId: '#(nodeTypeId)', canonicalName: '#(canonicalName)', description: '#(description)' }
    When method post
    Then status 201
