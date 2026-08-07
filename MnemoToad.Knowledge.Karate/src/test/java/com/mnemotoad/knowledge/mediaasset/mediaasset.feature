@Regression @MediaAsset
Feature: MediaAsset API

  Background:
    * url baseUrl
    * def uniqueName = read('classpath:com/mnemotoad/knowledge/common/util.js')
    * def mediaAssetFixtures = call read('fixtures.js')
    * def createMediaAsset = mediaAssetFixtures.create
    * configure afterScenario = mediaAssetFixtures.cleanup

  Scenario: Create a media asset successfully
    * def mediaUrl = 'https://example.com/' + uniqueName('MediaAsset') + '.svg'
    Given path 'mediaAssets'
    And request { url: '#(mediaUrl)' }
    When method post
    Then status 201
    And match response.url == mediaUrl
    And match response.id == '#uuid'
    * eval mediaAssetFixtures.stageForCleanup(response.id)

  Scenario: Reject creation with missing url
    Given path 'mediaAssets'
    And request { url: '' }
    When method post
    Then status 400

  Scenario: Get a media asset by id
    * def created = createMediaAsset()

    Given path 'mediaAssets', created.response.id
    When method get
    Then status 200
    And match response.url == created.response.url

  Scenario: Get a media asset that does not exist returns 404
    * def randomId = '' + java.util.UUID.randomUUID()
    Given path 'mediaAssets', randomId
    When method get
    Then status 404

  Scenario: Update a media asset
    * def created = createMediaAsset()
    * def updatedUrl = created.response.url + '_updated'

    Given path 'mediaAssets', created.response.id
    And request { url: '#(updatedUrl)' }
    When method put
    Then status 200
    And match response.url == updatedUrl

  Scenario: Update a media asset that does not exist returns 404
    * def randomId = '' + java.util.UUID.randomUUID()
    Given path 'mediaAssets', randomId
    And request { url: 'https://example.com/does-not-matter.svg' }
    When method put
    Then status 404

  Scenario: Delete a media asset
    * def created = createMediaAsset()

    Given path 'mediaAssets', created.response.id
    When method delete
    Then status 204

    Given path 'mediaAssets', created.response.id
    When method get
    Then status 404

  Scenario: Delete a media asset that does not exist returns 404
    * def randomId = '' + java.util.UUID.randomUUID()
    Given path 'mediaAssets', randomId
    When method delete
    Then status 404
