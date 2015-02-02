angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
    	$http.get("/api/policy").success(function (list) {
    		$scope.policyList = list;
    	});

    	$scope.updateReportTime = function (policySelected) {
    		$http.put("/api/policy", policySelected
    		).success(function () {
    			console.log("success");
    		}).error(function () {
    			console.log("error");
    		});
    	};
    }]);