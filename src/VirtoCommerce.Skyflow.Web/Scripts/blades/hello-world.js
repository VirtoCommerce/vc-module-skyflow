angular.module('Skyflow')
    .controller('Skyflow.helloWorldController', ['$scope', 'Skyflow.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'Skyflow';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'Skyflow.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
