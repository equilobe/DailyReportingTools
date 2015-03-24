'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/list/:instanceId', {
            label: 'Projects',
            templateUrl: 'app/projects.html',
            controller: 'ProjectCtrl'
        });
    }])
    .controller("ProjectCtrl", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $scope.status = "loading";

        $http.get("/api/project/" + $routeParams.instanceId)
            .success(function (list) {
                $scope.projects = list;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.updateReportTime = function ($scope) {
            if ($scope.reportForm.reportTime.$invalid || $scope.reportForm.reportTime.$pristine)
                return;

            $http.post("/api/policy", $scope.project
            ).success(function () {
                console.log("success");
            }).error(function () {
                console.log("error");
            });
        };
    }]);