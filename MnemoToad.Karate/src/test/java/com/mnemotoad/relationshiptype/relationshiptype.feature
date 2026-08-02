@Regression @RelationshipType
Feature: RelationshipType API

  Background:
    * url baseUrl
    * def uniqueName = read('classpath:com/mnemotoad/common/util.js')
    * def relationshipTypeFixtures = call read('fixtures.js')
    * def createRelationshipType = relationshipTypeFixtures.create
    * configure afterScenario = relationshipTypeFixtures.cleanup

  Scenario: Create a relationship type successfully
    * def name = uniqueName('RelationshipType')
    * def inverseName = 'inverseOf_' + name
    Given path 'relationshipTypes'
    And request { name: '#(name)', inverseName: '#(inverseName)', description: 'Created by Karate test' }
    When method post
    Then status 201
    And match response.name == name
    And match response.inverseName == inverseName
    And match response.description == 'Created by Karate test'
    And match response.id == '#uuid'
    * eval relationshipTypeFixtures.stageForCleanup(response.id)

  Scenario: Reject creation with missing name
    Given path 'relationshipTypes'
    And request { name: '', description: 'should fail' }
    When method post
    Then status 400

  Scenario: Reject creation with a duplicate name
    * def name = uniqueName('RelationshipType')
    * def created = createRelationshipType({ name: name })

    Given path 'relationshipTypes'
    And request { name: '#(name)' }
    When method post
    Then status 400

  Scenario: Get a relationship type by id
    * def created = createRelationshipType()

    Given path 'relationshipTypes', created.response.id
    When method get
    Then status 200
    And match response.name == created.response.name

  Scenario: List relationship types includes the newly created one
    * def created = createRelationshipType()

    Given path 'relationshipTypes'
    When method get
    Then status 200
    * def found = karate.filter(response, function(x){ return x.name == created.response.name })
    And match found[0].name == created.response.name

  Scenario: Update a relationship type
    * def created = createRelationshipType()

    * def updatedName = created.response.name + '_Updated'
    * def updatedInverseName = 'inverseOf_' + updatedName
    Given path 'relationshipTypes', created.response.id
    And request { name: '#(updatedName)', inverseName: '#(updatedInverseName)', description: 'Updated by test' }
    When method put
    Then status 200
    And match response.name == updatedName
    And match response.inverseName == updatedInverseName
    And match response.description == 'Updated by test'

  Scenario: Reject update with a duplicate name
    * def name1 = uniqueName('RelationshipType')
    * def name2 = uniqueName('RelationshipType')
    * def created1 = createRelationshipType({ name: name1 })
    * def created2 = createRelationshipType({ name: name2 })

    Given path 'relationshipTypes', created2.response.id
    And request { name: '#(name1)' }
    When method put
    Then status 400

  Scenario: Delete a relationship type
    * def created = createRelationshipType()

    Given path 'relationshipTypes', created.response.id
    When method delete
    Then status 204

    Given path 'relationshipTypes', created.response.id
    When method get
    Then status 404

  # TODO: no Relationship table/FK exists yet to reference a RelationshipType, so this scenario
  # can't be exercised for real. Un-ignore and implement once relationships are added, alongside
  # the corresponding delete-guard in RelationshipTypeService (see its DeleteAsync TODO).
  @ignore
  Scenario: Reject deleting a relationship type that is referenced by a relationship
    * def created = createRelationshipType()

    Given path 'relationshipTypes', created.response.id
    When method delete
    Then status 400
