angular.module("policyEditorApp", [])
    .controller("policyListPage", function ($scope, $http) {
        $http.get("api/policy").done(function(result){
            $scope.policyList = result;
        })        
    });