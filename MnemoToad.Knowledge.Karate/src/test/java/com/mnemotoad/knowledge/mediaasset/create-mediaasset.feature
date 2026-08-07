@ignore
Feature: Create a MediaAsset (reusable setup helper)

  Background:
    * url baseUrl

  Scenario:
    * def uniqueName = read('classpath:com/mnemotoad/knowledge/common/util.js')
    * def mediaUrl = karate.get('url') ? karate.get('url') : 'https://example.com/' + uniqueName('MediaAsset') + '.svg'
    Given path 'mediaAssets'
    And request { url: '#(mediaUrl)' }
    When method post
    Then status 201
