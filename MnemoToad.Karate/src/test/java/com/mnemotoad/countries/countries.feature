Feature: Country API

  @Smoke
  Scenario: Get all countries
    Given url baseUrl + '/countries'
    When method get
    Then status 200
    And match response == '#[]'