@ignore
Feature: Create a KnowledgeNode (reusable setup helper)

  Background:
    * url baseUrl

  Scenario:
    * def uniqueName = read('classpath:com/mnemotoad/knowledge/common/util.js')
    * def nodeTypeId = karate.get('nodeTypeId')
    * assert nodeTypeId != null
    * def canonicalName = karate.get('canonicalName') ? karate.get('canonicalName') : uniqueName('KnowledgeNode')
    * def description = karate.get('description')
    * def attributes = karate.get('attributes')
    Given path 'nodes'
    And request { nodeTypeId: '#(nodeTypeId)', canonicalName: '#(canonicalName)', description: '#(description)', attributes: '#(attributes)' }
    When method post
    Then status 201
