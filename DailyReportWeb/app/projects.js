'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/app/instances/:instanceId/projects', {
                templateUrl: 'app/projects.html',
                controller: 'ProjectsController'
            }).when('/app/projects', {
                templateUrl: 'app/projects.html',
                controller: 'ProjectsController'
            });
    }])
    .controller("ProjectsController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "projects");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/projects/")
            .success(function (instances) {
                $scope.instance = instances ? ($routeParams.instanceId ? instances[$routeParams.instanceId - 1] : instances[0]) : null;
                $scope.instances = instances;
            })
            .finally(function () {
                $scope.status = "loaded";
            });
    }]);