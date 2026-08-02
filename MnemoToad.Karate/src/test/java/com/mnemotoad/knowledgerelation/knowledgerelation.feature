@Regression @KnowledgeRelation
Feature: KnowledgeRelation API

  Background:
    * url baseUrl
    * def nodeTypeFixtures = call read('classpath:com/mnemotoad/nodetype/fixtures.js')
    * def knowledgeNodeFixtures = call read('classpath:com/mnemotoad/knowledgenode/fixtures.js')
    * def relationshipTypeFixtures = call read('classpath:com/mnemotoad/relationshiptype/fixtures.js')
    * def knowledgeRelationFixtures = call read('fixtures.js')
    * def createNodeType = nodeTypeFixtures.create
    * def createKnowledgeNode = knowledgeNodeFixtures.create
    * def createRelationshipType = relationshipTypeFixtures.create
    * def createKnowledgeRelation = knowledgeRelationFixtures.create
    * configure afterScenario =
      """
      function(){
        // Clean up dependents before dependencies: relations reference nodes and relationship
        // types, and nodes reference node types.
        knowledgeRelationFixtures.cleanup();
        knowledgeNodeFixtures.cleanup();
        nodeTypeFixtures.cleanup();
        relationshipTypeFixtures.cleanup();
      }
      """

  Scenario: Create a knowledge relation successfully
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    Given path 'relationships'
    And request { sourceNodeId: '#(sourceNode.response.id)', relationshipTypeId: '#(relationshipType.response.id)', targetNodeId: '#(targetNode.response.id)' }
    When method post
    Then status 201
    And match response.sourceNodeId == sourceNode.response.id
    And match response.relationshipTypeId == relationshipType.response.id
    And match response.targetNodeId == targetNode.response.id
    And match response.id == '#uuid'
    * eval knowledgeRelationFixtures.stageForCleanup(response.id)

  Scenario: Reject creation with missing source node id
    * def nodeType = createNodeType()
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    Given path 'relationships'
    And request { sourceNodeId: '00000000-0000-0000-0000-000000000000', relationshipTypeId: '#(relationshipType.response.id)', targetNodeId: '#(targetNode.response.id)' }
    When method post
    Then status 400

  Scenario: Reject creation with missing relationship type id
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    Given path 'relationships'
    And request { sourceNodeId: '#(sourceNode.response.id)', relationshipTypeId: '00000000-0000-0000-0000-000000000000', targetNodeId: '#(targetNode.response.id)' }
    When method post
    Then status 400

  Scenario: Reject creation with missing target node id
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    Given path 'relationships'
    And request { sourceNodeId: '#(sourceNode.response.id)', relationshipTypeId: '#(relationshipType.response.id)', targetNodeId: '00000000-0000-0000-0000-000000000000' }
    When method post
    Then status 400

  Scenario: Reject creation referencing a source node that does not exist
    * def nodeType = createNodeType()
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    * def randomId = '' + java.util.UUID.randomUUID()
    Given path 'relationships'
    And request { sourceNodeId: '#(randomId)', relationshipTypeId: '#(relationshipType.response.id)', targetNodeId: '#(targetNode.response.id)' }
    When method post
    Then status 400

  Scenario: Reject creation referencing a target node that does not exist
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    * def randomId = '' + java.util.UUID.randomUUID()
    Given path 'relationships'
    And request { sourceNodeId: '#(sourceNode.response.id)', relationshipTypeId: '#(relationshipType.response.id)', targetNodeId: '#(randomId)' }
    When method post
    Then status 400

  Scenario: Reject creation referencing a relationship type that does not exist
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def randomId = '' + java.util.UUID.randomUUID()
    Given path 'relationships'
    And request { sourceNodeId: '#(sourceNode.response.id)', relationshipTypeId: '#(randomId)', targetNodeId: '#(targetNode.response.id)' }
    When method post
    Then status 400

  Scenario: Reject creation of a duplicate relationship
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    * def created = createKnowledgeRelation({ sourceNodeId: sourceNode.response.id, relationshipTypeId: relationshipType.response.id, targetNodeId: targetNode.response.id })

    Given path 'relationships'
    And request { sourceNodeId: '#(sourceNode.response.id)', relationshipTypeId: '#(relationshipType.response.id)', targetNodeId: '#(targetNode.response.id)' }
    When method post
    Then status 400

  Scenario: Allow the same source and target under a different relationship type
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType1 = createRelationshipType()
    * def relationshipType2 = createRelationshipType()
    * def created1 = createKnowledgeRelation({ sourceNodeId: sourceNode.response.id, relationshipTypeId: relationshipType1.response.id, targetNodeId: targetNode.response.id })

    Given path 'relationships'
    And request { sourceNodeId: '#(sourceNode.response.id)', relationshipTypeId: '#(relationshipType2.response.id)', targetNodeId: '#(targetNode.response.id)' }
    When method post
    Then status 201
    * eval knowledgeRelationFixtures.stageForCleanup(response.id)

  Scenario: List relationships for a node as the source
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    * def created = createKnowledgeRelation({ sourceNodeId: sourceNode.response.id, relationshipTypeId: relationshipType.response.id, targetNodeId: targetNode.response.id })

    Given path 'nodes', sourceNode.response.id, 'relationships'
    When method get
    Then status 200
    * def foundIds = karate.map(response, function(x){ return x.id })
    And match foundIds contains created.response.id

  Scenario: List relationships for a node as the target
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    * def created = createKnowledgeRelation({ sourceNodeId: sourceNode.response.id, relationshipTypeId: relationshipType.response.id, targetNodeId: targetNode.response.id })

    Given path 'nodes', targetNode.response.id, 'relationships'
    When method get
    Then status 200
    * def foundIds = karate.map(response, function(x){ return x.id })
    And match foundIds contains created.response.id

  Scenario: Delete a knowledge relation
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    * def created = createKnowledgeRelation({ sourceNodeId: sourceNode.response.id, relationshipTypeId: relationshipType.response.id, targetNodeId: targetNode.response.id })

    Given path 'relationships', created.response.id
    When method delete
    Then status 204

    Given path 'nodes', sourceNode.response.id, 'relationships'
    When method get
    Then status 200
    * def foundIds = karate.map(response, function(x){ return x.id })
    And match foundIds !contains created.response.id

  Scenario: Reject deleting a knowledge node that is referenced by a relation
    * def nodeType = createNodeType()
    * def sourceNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def targetNode = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def relationshipType = createRelationshipType()
    * def created = createKnowledgeRelation({ sourceNodeId: sourceNode.response.id, relationshipTypeId: relationshipType.response.id, targetNodeId: targetNode.response.id })

    Given path 'nodes', sourceNode.response.id
    When method delete
    Then status 400
