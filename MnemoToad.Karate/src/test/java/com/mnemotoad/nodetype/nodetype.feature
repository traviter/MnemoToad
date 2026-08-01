@Regression
Feature: NodeType API

  Background:
    * url baseUrl
    * def uniqueName = read('classpath:com/mnemotoad/common/util.js')
    * def createdNodeTypeIds = []
    * configure afterScenario = read('cleanup.js')
    * def createNodeType =
      """
      function(overrides) {
        var args = overrides || {};
        var created = karate.call('create-nodetype.feature', args);
        createdNodeTypeIds.push(created.response.id);
        return created;
      }
      """

  Scenario: Create a node type successfully
    * def name = uniqueName('NodeType')
    Given path 'nodeTypes'
    And request { name: '#(name)', description: 'Created by Karate test' }
    When method post
    Then status 201
    And match response.name == name
    And match response.description == 'Created by Karate test'
    And match response.id == '#uuid'
    * eval createdNodeTypeIds.push(response.id)

  Scenario: Create multiple node types
    * def name1 = uniqueName('NodeType')
    * def name2 = uniqueName('NodeType')
    * def name3 = uniqueName('NodeType')

    Given path 'nodeTypes'
    And request { name: '#(name1)' }
    When method post
    Then status 201
    * eval createdNodeTypeIds.push(response.id)
    * def id1 = response.id

    Given path 'nodeTypes'
    And request { name: '#(name2)' }
    When method post
    Then status 201
    * eval createdNodeTypeIds.push(response.id)
    * def id2 = response.id

    Given path 'nodeTypes'
    And request { name: '#(name3)' }
    When method post
    Then status 201
    * eval createdNodeTypeIds.push(response.id)
    * def id3 = response.id

    * match id1 != id2
    * match id2 != id3
    * match id1 != id3

    Given path 'nodeTypes'
    When method get
    Then status 200
    * def foundNames = karate.map(karate.filter(response, function(x){ return x.name == name1 || x.name == name2 || x.name == name3 }), function(x){ return x.name })
    * match foundNames contains name1
    * match foundNames contains name2
    * match foundNames contains name3

  Scenario: Reject creation with missing name
    Given path 'nodeTypes'
    And request { name: '', description: 'should fail' }
    When method post
    Then status 400

  Scenario: Reject creation with a duplicate name
    * def name = uniqueName('NodeType')
    * def created = createNodeType({ name: name })

    Given path 'nodeTypes'
    And request { name: '#(name)' }
    When method post
    Then status 400

  Scenario: Get a node type by id
    * def created = createNodeType()

    Given path 'nodeTypes', created.response.id
    When method get
    Then status 200
    And match response.name == created.response.name

  Scenario: List node types includes the newly created one
    * def created = createNodeType()

    Given path 'nodeTypes'
    When method get
    Then status 200
    * def found = karate.filter(response, function(x){ return x.name == created.response.name })
    And match found[0].name == created.response.name

  Scenario: Update a node type
    * def created = createNodeType()

    * def updatedName = created.response.name + '_Updated'
    Given path 'nodeTypes', created.response.id
    And request { name: '#(updatedName)', description: 'Updated by test' }
    When method put
    Then status 200
    And match response.name == updatedName
    And match response.description == 'Updated by test'

  Scenario: Delete a node type
    * def created = createNodeType()

    Given path 'nodeTypes', created.response.id
    When method delete
    Then status 204

    Given path 'nodeTypes', created.response.id
    When method get
    Then status 404