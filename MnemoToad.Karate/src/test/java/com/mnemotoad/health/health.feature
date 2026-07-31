 @Smoke
  Feature: Health Check

    Scenario: API is up and healthy
      Given url baseUrl + '/health'
      When method get
      Then status 200
      And match response == {status: "pass"}