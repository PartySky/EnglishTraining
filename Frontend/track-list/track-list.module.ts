import * as angular from "angular";

import TrackListTemplate from "./components/track-list.html";
import { TrackListComponent } from "./components/track-list.component";
// import { ApiService } from "./services/api.service";


export const name = "EnglishTraining.TrackList.components";
angular
    .module(name, [])
    .component("etTrackList", {
        template: TrackListTemplate,
        controller: TrackListComponent
    });
    // .service('ApiService', ApiService);

// angular.module('EnglishTraining.TrackList.components')
//     .service('ApiService', ApiService);