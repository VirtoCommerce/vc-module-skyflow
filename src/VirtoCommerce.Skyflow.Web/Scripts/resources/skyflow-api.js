angular.module('Skyflow')
    .factory('Skyflow.webApi', ['$resource', function ($resource) {
        return $resource('api/skyflow');
    }]);
