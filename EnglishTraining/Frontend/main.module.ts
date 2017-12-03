import * as angular from "angular";

import * as trackListComponentsModule from "./tracklist/tracklist.module";

angular
    .module('EnglishTraining.fullstack', [
        trackListComponentsModule.name,
    ]);