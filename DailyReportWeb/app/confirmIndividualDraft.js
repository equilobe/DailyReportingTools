'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/report/confirmIndividualDraft/:id', {
            templateUrl: 'app/confirmIndividualDraft.html',
            controller: 'ConfirmIndividualDraftController'
        });
    }])
    .controller("ConfirmIndividualDraftController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "confirmIndividualDraft");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.post("/api/confirmIndividualDraft/", $routeParams)
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