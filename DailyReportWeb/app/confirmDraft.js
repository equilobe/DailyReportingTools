'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/report/confirmDraft/:id', {
            templateUrl: 'app/confirmDraft.html',
            controller: 'ConfirmDraftController'
        });
    }])
    .controller("ConfirmDraftController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "confirmDraft");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.post("/api/confirmDraft/", $routeParams)
            .success(function (response) {
                $scope.data = response;
            })
            .error(function () {
                $scope.error = "Something went wrong, request is not valid";
            })
            .finally(function () {
                $scope.status = "loaded";
            });
    }]);