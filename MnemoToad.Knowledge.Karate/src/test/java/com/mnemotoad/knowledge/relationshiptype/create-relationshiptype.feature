@ignore
Feature: Create a RelationshipType (reusable setup helper)

  Background:
    * url baseUrl

  Scenario:
    * def uniqueName = read('classpath:com/mnemotoad/knowledge/common/util.js')
    * def name = karate.get('name') ? karate.get('name') : uniqueName('RelationshipType')
    * def inverseName = karate.get('inverseName')
    * def description = karate.get('description')
    Given path 'relationshipTypes'
    And request { name: '#(name)', inverseName: '#(inverseName)', description: '#(description)' }
    When method post
    Then status 201
