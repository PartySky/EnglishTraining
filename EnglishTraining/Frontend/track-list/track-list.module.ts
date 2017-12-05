import * as angular from "angular";

import TrackListTemplate from "./components/track-list.html";
import { TrackListComponent } from "./components/track-list.component";

export const name = "EnglishTraining.TrackList.components";
angular
    .module(name, [])
    .component("etTrackList", {
        template: TrackListTemplate,
        controller: TrackListComponent
    });