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
        $scope.status = "";
        $scope.pageStatus = "loading";
        $scope.pageHasError = false;

        var resetPasswordDetails = {
            userId: $routeParams.userId,
            code: encodeURIComponent($routeParams.code),
            newPassword: ""
        };

        $http.post("/api/account/resetPassword", resetPasswordDetails)
            .success(function (response) {
                if (response.hasError) {
                    $scope.pageHasError = true;
                    $scope.message = response.message;
                }
            })
            .error(function () {
                $scope.pageHasError = true;
                $scope.message = "Cannot reset password";
            })
            .finally(function () {
                $scope.pageStatus = "loaded";
            });

        $scope.changePassword = function ($scope) {
            resetPasswordDetails.newPassword = $scope.form.password;
            if ($scope.form.password != $scope.form.confirmedPassword)
            {
                $scope.message = "Passwords don't match!";
                $scope.status = "error";
                $scope.form.$setPristine();
                return false;
            }

            $http.post("/api/account/changePassword", resetPasswordDetails)
                .success(function (response) {
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
                })
                .finally(function () {
                    $scope.form.$setPristine();
                })
            }
    }]);