import * as angular from "angular";

import TrackListTemplate from "./tracklist.html";
import { TrackListComponent } from "./tracklist.component";

export const name = "wave.chart.fullstack.components";
angular
    .module(name, [])
    .component("etTrackList", {
        template: TrackListTemplate,
        controller: TrackListComponent
    });