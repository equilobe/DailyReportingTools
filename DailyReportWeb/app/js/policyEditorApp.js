angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
        $http.get("/api/policy").success(function(list){
            $scope.policyList = list;
        })        
    }]);