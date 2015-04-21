'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/report/sendIndividualDraft/:id', {
            templateUrl: 'app/sendIndividualDraft.html',
            controller: 'SendIndividualDraftController'
        });
    }])
    .controller("SendIndividualDraftController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "sendIndividualDraft");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.post("/api/sendIndividualDraft", $routeParams)
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