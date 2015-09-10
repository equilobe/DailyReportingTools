'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/resetPassword', {
            templateUrl: 'app/resetPassword.html',
            controller: 'ResetPasswordController'
        });
    }])
    .controller("ResetPasswordController", ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "resetPassword");
        $scope.status = "loading";

        var resetPasswordDetails = {
            userId: $routeParams.userId,
            code: encodeURIComponent($routeParams.code),
            newPassword: ""
        };

        $http.post("/api/account/resetPassword", resetPasswordDetails)
            .succes(function (response) {
                if (response.hasError) {
                    $scope.status = "error";
                    $scope.message = response.message;
                }
            })
            .error(function () {
                $scope.status = "error";
                $scope.message = "Cannot reset password";
            });

        $scope.changePassword = function ($scope) {
            resetPasswordDetails.newPassword = $scope.newPassword;
            $http.post("/api/account/changePassword", resetPasswordDetails)
                .succes(function (response) {
                    if (response.hasError) {
                        $scope.status = "error";
                        $scope.message = response.message;
                    }
                    else {
                        $scope.status = "success";
                        $scope.message = response.message;
                    }
                })
                .error(function () {
                    $scope.status = "error";
                });
            }
    }]);