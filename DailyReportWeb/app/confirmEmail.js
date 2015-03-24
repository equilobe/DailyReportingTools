'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/confirmEmail', { templateUrl: 'app/confirmEmail.html', controller: 'ConfirmEmailCtrl' });
    }])
    .controller("ConfirmEmailCtrl", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $scope.status = "loading";
        var confirmationDetails = {
            userId: $routeParams.userId,
            code: encodeURIComponent($routeParams.code)
        };

        $http.post("/api/account/confirmEmail", confirmationDetails)
            .success(function (response) {
                if (response.success)
                    $scope.isConfirmed = true;
            })
            .finally(function () {
                $scope.status = "loaded";
            });
    }]);