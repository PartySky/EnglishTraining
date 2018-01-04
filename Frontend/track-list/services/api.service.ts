import { VmWord } from "../components/models/VmWord";
// import * as angular from "angular";

/* ngInject */
export class ApiService {
    private readonly _http: ng.IHttpService;
    private readonly _apiUrl: string;

    constructor(
        http: ng.IHttpService
    ) {
        this._http = http;
        this._apiUrl = "/Main/GetWords";
    }
    get() {
        return this._http
            .get<VmWord>(`${this._apiUrl}`)
            .then(response => response.data);
    }
}

// angular.module('EnglishTraining.TrackList.components')
//     .service('ApiService', ApiService);