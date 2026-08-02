@Regression
Feature: KnowledgeNode API

  Background:
    * url baseUrl
    * def uniqueName = read('classpath:com/mnemotoad/common/util.js')
    * def nodeTypeFixtures = call read('classpath:com/mnemotoad/nodetype/fixtures.js')
    * def knowledgeNodeFixtures = call read('fixtures.js')
    * def createNodeType = nodeTypeFixtures.create
    * def createKnowledgeNode = knowledgeNodeFixtures.create
    * configure afterScenario =
      """
      function(){
        // KnowledgeNodes must be deleted before their referenced NodeTypes -- the FK on
        // knowledge_node.node_type_id would otherwise reject the NodeType delete.
        knowledgeNodeFixtures.cleanup();
        nodeTypeFixtures.cleanup();
      }
      """

  Scenario: Create a knowledge node successfully
    * def nodeType = createNodeType()
    * def name = uniqueName('KnowledgeNode')
    Given path 'nodes'
    And request { nodeTypeId: '#(nodeType.response.id)', canonicalName: '#(name)', description: 'Created by Karate test' }
    When method post
    Then status 201
    And match response.nodeTypeId == nodeType.response.id
    And match response.canonicalName == name
    And match response.description == 'Created by Karate test'
    And match response.id == '#uuid'
    * eval knowledgeNodeFixtures.stageForCleanup(response.id)

  Scenario: Reject creation with missing canonical name
    * def nodeType = createNodeType()
    Given path 'nodes'
    And request { nodeTypeId: '#(nodeType.response.id)', canonicalName: '' }
    When method post
    Then status 400

  Scenario: Reject creation with missing node type id
    Given path 'nodes'
    And request { nodeTypeId: '00000000-0000-0000-0000-000000000000', canonicalName: '#(uniqueName("KnowledgeNode"))' }
    When method post
    Then status 400

  Scenario: Reject creation referencing a node type that does not exist
    * def randomId = '' + java.util.UUID.randomUUID()
    Given path 'nodes'
    And request { nodeTypeId: '#(randomId)', canonicalName: '#(uniqueName("KnowledgeNode"))' }
    When method post
    Then status 400

  Scenario: Reject creation with a duplicate name for the same node type
    * def nodeType = createNodeType()
    * def name = uniqueName('KnowledgeNode')
    * def created = createKnowledgeNode({ nodeTypeId: nodeType.response.id, canonicalName: name })

    Given path 'nodes'
    And request { nodeTypeId: '#(nodeType.response.id)', canonicalName: '#(name)' }
    When method post
    Then status 400

  Scenario: Allow the same canonical name under different node types
    * def nodeType1 = createNodeType()
    * def nodeType2 = createNodeType()
    * def name = uniqueName('KnowledgeNode')
    * def created1 = createKnowledgeNode({ nodeTypeId: nodeType1.response.id, canonicalName: name })

    Given path 'nodes'
    And request { nodeTypeId: '#(nodeType2.response.id)', canonicalName: '#(name)' }
    When method post
    Then status 201
    * eval knowledgeNodeFixtures.stageForCleanup(response.id)

  Scenario: Get a knowledge node by id
    * def nodeType = createNodeType()
    * def created = createKnowledgeNode({ nodeTypeId: nodeType.response.id })

    Given path 'nodes', created.response.id
    When method get
    Then status 200
    And match response.canonicalName == created.response.canonicalName

  Scenario: List knowledge nodes includes the newly created one
    * def nodeType = createNodeType()
    * def created = createKnowledgeNode({ nodeTypeId: nodeType.response.id })

    Given path 'nodes'
    When method get
    Then status 200
    * def found = karate.filter(response, function(x){ return x.id == created.response.id })
    And match found[0].canonicalName == created.response.canonicalName

  Scenario: List knowledge nodes filtered by node type
    * def nodeType1 = createNodeType()
    * def nodeType2 = createNodeType()
    * def created1 = createKnowledgeNode({ nodeTypeId: nodeType1.response.id })
    * def created2 = createKnowledgeNode({ nodeTypeId: nodeType2.response.id })

    Given path 'nodes'
    And param nodeTypeId = nodeType1.response.id
    When method get
    Then status 200
    * def foundIds = karate.map(response, function(x){ return x.id })
    And match foundIds contains created1.response.id
    And match foundIds !contains created2.response.id

  Scenario: Update a knowledge node
    * def nodeType = createNodeType()
    * def created = createKnowledgeNode({ nodeTypeId: nodeType.response.id })

    * def updatedName = created.response.canonicalName + '_Updated'
    Given path 'nodes', created.response.id
    And request { nodeTypeId: '#(created.response.nodeTypeId)', canonicalName: '#(updatedName)', description: 'Updated by test' }
    When method put
    Then status 200
    And match response.canonicalName == updatedName
    And match response.description == 'Updated by test'

  Scenario: Reject update with a duplicate name for the same node type
    * def nodeType = createNodeType()
    * def name1 = uniqueName('KnowledgeNode')
    * def name2 = uniqueName('KnowledgeNode')
    * def created1 = createKnowledgeNode({ nodeTypeId: nodeType.response.id, canonicalName: name1 })
    * def created2 = createKnowledgeNode({ nodeTypeId: nodeType.response.id, canonicalName: name2 })

    Given path 'nodes', created2.response.id
    And request { nodeTypeId: '#(nodeType.response.id)', canonicalName: '#(name1)' }
    When method put
    Then status 400

  Scenario: Reject update referencing a node type that does not exist
    * def nodeType = createNodeType()
    * def created = createKnowledgeNode({ nodeTypeId: nodeType.response.id })
    * def randomId = '' + java.util.UUID.randomUUID()

    Given path 'nodes', created.response.id
    And request { nodeTypeId: '#(randomId)', canonicalName: '#(created.response.canonicalName)' }
    When method put
    Then status 400

  Scenario: Delete a knowledge node
    * def nodeType = createNodeType()
    * def created = createKnowledgeNode({ nodeTypeId: nodeType.response.id })

    Given path 'nodes', created.response.id
    When method delete
    Then status 204

    Given path 'nodes', created.response.id
    When method get
    Then status 404

  Scenario: Reject deleting a node type that is referenced by a knowledge node
    * def nodeType = createNodeType()
    * def created = createKnowledgeNode({ nodeTypeId: nodeType.response.id })

    Given path 'nodeTypes', nodeType.response.id
    When method delete
    Then status 400
