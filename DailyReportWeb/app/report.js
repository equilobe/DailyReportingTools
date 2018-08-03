'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/instances/:instanceId/report', {
            templateUrl: 'app/report.html',
            controller: 'ReportController'
        });
    }])
    .controller("ReportController", ["$scope", "$http", '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "report");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/report/" + $routeParams.instanceId)
            .success(function (data) {
                $scope.data = data;
            })
            .finally(function () {
                $scope.status = "loaded";
            });
    }])
