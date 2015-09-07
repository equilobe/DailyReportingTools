'use strict';
angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/instances', {
            templateUrl: 'app/instances.html',
            controller: 'InstancesController'
        });
    }])

    .controller("InstancesController", ['$scope', '$http', function ($scope, $http) {
        $("body").attr("data-page", "instances");
        $scope.$parent.child = $scope;
        $scope.status = "loading";
        $scope.subscribePhase = false;
        $scope.serializedForm = {};
        $scope.instanceUrl = "";
        $scope.isSubscriptionOnTrial = false;

        $http.get("/api/instances")
            .success(function (list) {
                list.forEach(function (item) {
                    item.hostname = item.baseUrl.substring(item.baseUrl.indexOf('/') + 2, item.baseUrl.length).split('/')[0];
                });

                $scope.instances = list;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.addInstance = function ($scope) {
            $scope.addingInstance = true;
            $scope.form.timeZone = $scope.timeZone;
        };

        $scope.editInstance = function ($scope) {
            $scope.$parent.$parent.editingInstance = true;
            $scope.$parent.$parent.instanceUrl = $scope.instance.baseUrl;
            $scope.form.baseUrl = $scope.instance.baseUrl;
            $scope.form.timeZone = $scope.instance.timeZone;

            if (!$scope.instance.isActive) {
                $scope.$parent.$parent.subscribePhase = true;
                $scope.$parent.$parent.serializedForm = {
                    instanceId: $scope.instance.id,
                    baseUrl: $scope.instance.baseUrl
                };
                $scope.setIsOnTrial($scope.instance.id);
            }
        };

        $scope.setIsOnTrial = function (instanceId) {
            $http.get("/api/instances/" + instanceId)
            .success(function (isOnTrial) {
                $scope.isSubscriptionOnTrial = isOnTrial;
            })
            .error(function (error) {
                console.log("Error");
            });
        };

        $scope.backToInstanceEdit = function ($scope) {
            $scope.subscribePhase = false;
        }

        //$scope.deleteInstance = function ($scope) {
        //    var confirmation = true;
        //    if ($scope.instance.isActive) {
        //        confirmation = confirm("Are you sure you want to remove instance? \n" + $scope.instance.baseUrl + "\nThe subscription period is not over yet.");
        //    }
        //    else {
        //        confirmation = confirm("Are you sure you want to remove instance ?\n" + $scope.instance.baseUrl);
        //    }

        //    if (confirmation) {
        //        $scope.child.status = "loading";

        //        $http.delete("/api/instances/" + $scope.instance.id)
        //            .success(function (list) {
        //                $scope.child.instances = list;
        //            })
        //            .finally(function () {
        //                $scope.child.status = "loaded";
        //            });
        //    }
        //};

        $scope.clearInstanceForm = function ($scope) {
            $scope.form.$setPristine();
            $scope.form.baseUrl = "";
            $scope.form.jiraUsername = "";
            $scope.form.jiraPassword = "";
            $scope.status = "";
            $scope.message = "";
            $scope.addingInstance = false;
            $scope.editingInstance = false;
            $scope.subscribePhase = false;
        };

        $scope.saveInstance = function ($scope) {
            $scope.status = 'saving';
            $scope.form.$setPristine();
            if ($scope.editingInstance) {
                $scope.updateInstance($scope);
            }
            else {
                $scope.saveNewInstance($scope);
            }
        };

        $scope.updateInstance = function ($scope) {
            var updateInstaceModel = $scope.getUpdateInstanceModel($scope.form);
            $http.post("api/instances/updateInstance", updateInstaceModel)
                 .success(function (instance) {
                     $scope.clearInstanceForm($scope);
                     $scope.status = "succes";
                     $scope.message = "Updated";
                 })
                 .error(function (error) {
                     $scope.message = "Invalid JIRA username or password";
                     $scope.status = "error";
                 });
        };

        $scope.saveNewInstance = function ($scope) {
            var newInstanceHostname = $scope.form.baseUrl.substring($scope.form.baseUrl.indexOf('/') + 2, $scope.form.baseUrl.length).split('/')[0];
            $scope.instances.forEach(function (instance) {
                if (instance.baseUrl.indexOf(newInstanceHostname) != -1) {
                    $scope.message = "Jira server is already present!";
                    $scope.status = "error";
                    return;
                }
            });

            if ($scope.status == "error") {
                $scope.subscribePhase = false;
                return;
            }

            $http.post("/api/instances/checkInstanceCredentials", $scope.form)
                 .success(function (instance) {
                     // $scope.$parent.instances = list;
                     //  $scope.clearInstanceForm($scope);
                     $scope.status = "success";
                     $scope.message = "Please subscribe for the Jira instance. It may take a few minutes to process the subscription.";
                     $scope.serializedForm =
                     {
                         baseUrl: $scope.form.baseUrl,
                         timeZone: $scope.form.timeZone,
                         jiraUsername: $scope.form.jiraUsername,
                         jiraPassword: $scope.form.jiraPassword,
                         email: instance.email
                     };
                     $scope.instanceUrl = $scope.form.baseUrl;
                     $scope.subscribePhase = true;
                 })
                 .error(function (error) {
                     $scope.message = "Invalid JIRA username or password";
                     $scope.status = "error";
                     $scope.subscribePhase = false;
                 });
        };

        $scope.getUpdateInstanceModel = function (registerForm) {
            return{
                baseUrl: registerForm.baseUrl,
                timeZone: registerForm.timeZone,
                jiraUsername: registerForm.jiraUsername,
                jiraPassword: registerForm.jiraPassword,
                updateInstance: true
            };
        };
    }]);