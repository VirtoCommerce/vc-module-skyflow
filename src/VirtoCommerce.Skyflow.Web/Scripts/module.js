// Call this to register your module to main application
var moduleName = 'Skyflow';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.SkyflowState', {
                    url: '/Skyflow',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'Skyflow.helloWorldController',
                                template: 'Modules/$(VirtoCommerce.Skyflow)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', '$state',
        function (mainMenuService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/Skyflow',
                icon: 'fa fa-cube',
                title: 'Skyflow',
                priority: 100,
                action: function () { $state.go('workspace.SkyflowState'); },
                permission: 'Skyflow:access',
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
