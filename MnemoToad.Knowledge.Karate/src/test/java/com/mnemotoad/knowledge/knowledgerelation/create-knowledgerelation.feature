@ignore
Feature: Create a KnowledgeRelation (reusable setup helper)

  Background:
    * url baseUrl

  Scenario:
    * def sourceNodeId = karate.get('sourceNodeId')
    * def relationshipTypeId = karate.get('relationshipTypeId')
    * def targetNodeId = karate.get('targetNodeId')
    * assert sourceNodeId != null
    * assert relationshipTypeId != null
    * assert targetNodeId != null
    Given path 'relationships'
    And request { sourceNodeId: '#(sourceNodeId)', relationshipTypeId: '#(relationshipTypeId)', targetNodeId: '#(targetNodeId)' }
    When method post
    Then status 201
