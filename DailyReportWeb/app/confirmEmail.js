'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/confirmEmail', {
            templateUrl: 'app/confirmEmail.html',
            controller: 'ConfirmEmailController'
        });
    }])
    .controller("ConfirmEmailController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "confirmEmail");
        $scope.$parent.child = $scope;
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