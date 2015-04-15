'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/report/confirmDraft/:executionContext', {
            templateUrl: 'app/confirmDraft.html',
            controller: 'ConfirmDraftController'
        });
    }])
    .controller("ConfirmDraftController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "confirmDraft");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/confirmDraft/" + $routeParams.executionContext)
            .success(function (response) {
                $scope.response = response;
            })
            .finally(function () {
                $scope.status = "loaded";
            });
    }]);