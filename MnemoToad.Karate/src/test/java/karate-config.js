function fn() {
    var env = karate.env || 'dev';
    var config = {
        baseUrl: 'https://localhost:7117'
    };

    if (env === 'azure') {
        config.baseUrl = 'https://mnemotoad-fybvhxgdapesd2d3.westus3-01.azurewebsites.net/';
    }

    if (env === 'dev') {
        karate.configure('ssl', { trustAll: true });
    }

    return config;
}