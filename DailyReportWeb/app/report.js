'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/instances/:instanceId/report', {
            templateUrl: 'app/report.html',
            controller: 'ReportController'
        });
    }])
    .controller("ReportController", ["$scope", "$http", '$routeParams',
        function reportCtrl($scope, $http, $routeParams) {
            var ctrl = this;
            ctrl.isLoading = true;
            ctrl.data = {};

            $http.get("/api/report/" + $routeParams.instanceId)
                .success(function (data) {
                    ctrl.data = data;
                })
                .finally(function () {
                    ctrl.isLoading = false;
                });
    }])
