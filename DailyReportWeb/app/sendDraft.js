'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/report/sendDraft/:id', {
            templateUrl: 'app/sendDraft.html',
            controller: 'SendDraftController'
        });
    }])
    .controller("SendDraftController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "sendDraft");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.post("/api/sendDraft", $routeParams)
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