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
        };

        $scope.signInPhase = function ($scope) {
            $scope.$parent.forgotPasswordPhase = false;
            $scope.$parent.status = "";
            $scope.$parent.message = "";
        };

        $scope.sendMailToResetPassword = function ($scope) {
            $http.post("/api/account/sendResetPasswordMail", $scope.form)
              .success(function (response) {
                  if (!response.hasError) {
                      $scope.$parent.message = "Details for resetting your password have been sent to " + $scope.form.email + " email adress";
                      $scope.$parent.status = "success";
                  }
                  else {
                      $scope.$parent.message = response.message;
                      $scope.$parent.status = "error";
                  }
              })
            .error(function () {
                $scope.$parent.status = "error"
            });
        };

    }]);