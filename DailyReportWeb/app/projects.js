'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/instances/:instanceId/projects', {
            label: 'Projects',
            templateUrl: 'app/projects.html',
            controller: 'ProjectsController'
        });
    }])
    .controller("ProjectsController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "projects");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/projects/" + $routeParams.instanceId)
            .success(function (list) {
                $scope.projects = list;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.updateReportTime = function ($scope) {
            if ($scope.reportForm.reportTime.$invalid || $scope.reportForm.reportTime.$pristine)
                return;

            $http.post("/api/projects", $scope.project
            ).success(function () {
                console.log("success");
            }).error(function () {
                console.log("error");
            });
        };
    }]);