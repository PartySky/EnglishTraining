import * as angular from "angular";

import * as TrackList from "./track-list/track-list.module";

angular
    .module('EnglishTraining', [
        TrackList.name,
    ])
    // ;
    // .controller('ExampleController', ['$scope', '$document', function ($scope, $document) {
    //     $scope.title = $document[0].title;
    //     $scope.windowTitle = angular.element(window.document)[0].title;
    // }]);
    .controller('ExampleController', ['$scope', '$document', function ($scope, $document) {
        $scope.title = $document[0].title;
        $scope.windowTitle = angular.element(window.document)[0].title;
        $scope.check2 = function () {
            console.log("check2()");
            console.log();
        }
    }]);
    // .controller('myCtrl', ['$scope', function ($scope) {
    //     $scope.count = 0;
    //     $scope.myFunc = function () {
    //         $scope.count++;
    //     };
    // }]);