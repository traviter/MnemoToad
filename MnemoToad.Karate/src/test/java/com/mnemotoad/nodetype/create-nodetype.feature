Feature: Create a NodeType (reusable setup helper)

  Background:
    * url baseUrl

  Scenario:
    * def uniqueName = read('classpath:com/mnemotoad/common/util.js')
    * def name = karate.get('name') ? karate.get('name') : uniqueName('NodeType')
    * def description = karate.get('description')
    Given path 'nodeTypes'
    And request { name: '#(name)', description: '#(description)' }
    When method post
    Then status 201