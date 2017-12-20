import * as angular from "angular";

import * as TrackList from "./track-list/track-list.module";

angular
    .module('EnglishTraining', [
        TrackList.name,
    ])
