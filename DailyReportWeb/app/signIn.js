'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/signin', {
            templateUrl: 'app/signIn.html',
            controller: 'SignInController'
        });
    }])
    .controller("SignInController", ['$scope', '$http', '$location', function ($scope, $http, $location) {
        $("body").attr("data-page", "signin");
        $scope.$parent.child = $scope;
        $scope.forgotPasswordPhase = false;

        $scope.signIn = function ($scope) {
            $scope.status = "checking";
            $scope.form.$setPristine();

            $http.post("/api/account/login", $scope.form)
                .success(function (response) {
                    if (!response.hasError) {
                        $scope.$root.isAuth = true;
                        $location.path('/app/instances/0/projects');
                    }
                    else {
                        $scope.message = response.message;
                        $scope.status = "error";
                    }
                })
                .error(function () {
                    $scope.status = "error";
                });
        };

        $scope.forgotPassword = function ($scope) {
            $scope.forgotPasswordPhase = true;
            $scope.form.$setDirty();
        };

        $scope.switchToSignInPhase = function ($scope) {
            $scope.forgotPasswordPhase = false;
            $scope.status = "";
            $scope.message = "";
            $scope.form.$setDirty();
        };

        $scope.sendMailToResetPassword = function ($scope) {
            $scope.status = "checking";
            $http.post("/api/account/sendResetPasswordMail", $scope.form)
              .success(function (response) {
                  if (!response.hasError) {
                      $scope.message = "Details for resetting your password have been sent to " + $scope.form.email + " email adress";
                      $scope.status = "success";
                  }
                  else {
                      $scope.message = response.message;
                      $scope.status = "error";
                  }
              })
            .error(function () {
                $scope.status = "error"
            })
            .finally(function () {
                $scope.form.$setPristine();
            });
        };

    }]);