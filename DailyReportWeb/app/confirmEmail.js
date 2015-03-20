'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/confirmEmail', { templateUrl: 'app/confirmEmail.html', controller: 'ConfirmEmailCtrl' });
    }])
    .controller("ConfirmEmailCtrl", ['$scope', '$http', '$routeParams', '$sce', '$location', function ($scope, $http, $routeParams, $sce, $location) {
        $scope.status = "loading";
        $scope.isConfirmed = false;
        var confirmationDetails = { userId: $routeParams.userId, code: $sce.trustAsResourceUrl($routeParams.code) }
        $http.post("/api/account/confirmEmail", confirmationDetails)
            .success(function (response) {
                if (response.success)
                    $scope.isConfirmed = true;
            })
            .finally(function () {
                $scope.status = "loaded";
            });
    }]);