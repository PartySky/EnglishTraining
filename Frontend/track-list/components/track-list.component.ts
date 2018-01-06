import { VmWord } from "./models/VmWord";
import { VmWordExtended } from "./models/VmWordExtended";
import { VmAudioPath } from "./models/VmAudioPath";
import { name } from "../track-list.module";
// import { ApiService } from "../services/api.service";

export class TrackListComponent {
    // private readonly _http: ng.IHttpService;
    private readonly _apiUrl: string;
    // private readonly _apiSrv: ApiService;
    private _audioPath: VmAudioPath = {
        "en": "http://wooordhunt.ru/data/sound/word/uk/mp3/",
        "ru": "./audio/"
    }
    private _currentLocal: string;
    private _currentTime: number;
    private _currentWord: VmWordExtended;
    private _spentTime: number = 0;
    private _words: VmWordExtended[];
    private _keyNextWord: number = 32;
    private _keyStop: number = 13;
    private _highRateLearn: number = 48;
    spentTimeToShow: string;
    autoSaveTimerPrevious: number;
    count: number = 0;
    fileToPlay: string;
    wordToShow: string;

    constructor(
        // http: ng.IHttpService,
        private $http: ng.IHttpService,
        
        public $rootScope: ng.IRootScopeService,
        // apiService: ApiService
    ) {
        // this._http = http;
        this._apiUrl = "/main/word";
        // this._apiSrv = apiService;
        this.getWords()
            .then((words) => {
                this._words = words
                    .map((word: VmWordExtended) => ({
                        ...word,
                        Name: {
                            en: word.name_en,
                            ru: word.name_ru
                        }
                    }));
            });
        document.addEventListener("keydown", (e) => this.keyDownTextField(e), false);
        this.autoSaveTimerPrevious = this.getSecondsToday();
    }

    getWords() {
        // TODO: move to services
        return this.$http
            .get<VmWord[]>(`${this._apiUrl}`)
            // .get<VmWord[]>(`${this._apiUrl}/getwords`)
            .then(response => response.data);
    }

    updateWord(word: VmWord) {
        return this.$http
            .post<string>(`${this._apiUrl}/update`, word)
            .then(response => response.data);
    }

    autoSave() {
        let autoSaveTimer = this.getSecondsToday() - this.autoSaveTimerPrevious ;
        const timerAmmount: number = 15;
        if (autoSaveTimer > timerAmmount) {
            this._words.forEach(word => {
                this.updateWord(word);
            });
            console.log("Auto Save!!!");
            this.autoSaveTimerPrevious = this.getSecondsToday();
        }
    }

    keyDownTextField(e: any) {
        this.autoSave();
        let keyCode = e.keyCode;
        let today = new Date;
        this.calculateSpentTime();
        if (keyCode === this._keyNextWord) {
            if (!this._words[0].CurrentRandomLocalization) {
                this._currentLocal = this.getRandomLocal();
            } else {
                this._currentLocal = this._words[0].CurrentRandomLocalization;
                this._words[0].CurrentRandomLocalization = null;
            }
            this.wordToShow = null;
            if (this._currentWord) {
                this._words.push(this._currentWord);
            }
            this._currentWord = this._words[0];
            // TODO: set repeat count after right answer, not before
            if (this._currentLocal == "en") {
                this._currentWord.dailyReapeatCountForEng++;
            } else {
                this._currentWord.dailyReapeatCountForRus++;
            }

            this.fileToPlay = this._audioPath[this._currentLocal] + this._words[0].Name[this._currentLocal] + ".mp3";

            console.log("cureent word: " + this._words[0].Name[this._currentLocal]);

            this._words.shift();
            this.play();
            this.logElements();
        }
        if ((keyCode === this._keyStop && this._currentWord) ||
            (keyCode === 16 && this._currentWord) ||
            (keyCode === this._highRateLearn && this._currentWord)) {
            // if (keyCode == this.keyStop && this._currentWord) {
            let numberToSplice: number;
            let invertedLang = this.invertLanguage(this._currentLocal);

            let thirdPartOfWordsLenght: number = this._words.length / 3;
            if (keyCode === this._highRateLearn) {
                numberToSplice = this.getRandomNumber(4, 8);
            } else {
                numberToSplice = this.getRandomNumber(thirdPartOfWordsLenght,
                    thirdPartOfWordsLenght * 2);
            }

            this._currentWord.CurrentRandomLocalization = this._currentLocal;
            this.wordToShow = this._currentWord.Name[invertedLang];
            this._words.splice(numberToSplice, 0, this._currentWord);

            console.log("Word ToShow = " + this.wordToShow);

            this.fileToPlay = this._audioPath[invertedLang] +
                this._currentWord.Name[invertedLang] + ".mp3";

            this._currentWord = null;

            if (keyCode == 16) {
                this.play();
            }
            this.logElements();
        }
    }

    play() {
        var audio = new Audio(this.fileToPlay);
        audio.play();
        console.log(this.fileToPlay);
    }

    logElements() {
        console.log("");
        let stringOfWords: string = "";
        this._words.forEach(w => {
            stringOfWords = stringOfWords + " " + w.Name["ru"];
            if (w.CurrentRandomLocalization) {
                stringOfWords = stringOfWords + "_Has_Stored_local:" + w.CurrentRandomLocalization;
            }
        });
        console.log(stringOfWords);
        this.$rootScope.$apply();
    }

    getRandomNumber(min: number, max: number) {
        return Math.random() * (max - min) + min;
    }

    getRandomLocal() {
        let num = Math.round(this.getRandomNumber(0, 1));
        switch (num) {
            case 0:
                return "en";
            case 1:
                return "ru";
        }

    }

    invertLanguage(lang: string) {
        if (lang == "en") {
            return "ru";
        } else {
            return "en";
        }
    }

    getSecondsToday() {
        var d = new Date();
        return d.getHours() * 3600 + d.getMinutes() * 60 + d.getSeconds();
    };

    calculateSpentTime() {
        let timeDiff = (this.getSecondsToday() - this._currentTime);
        if (timeDiff < 15) {
            this._spentTime = this._spentTime + timeDiff;
        }
        let min = Math.floor(this._spentTime / 60);
        let sec = this._spentTime - min * 60;
        this.spentTimeToShow = min + " : " + sec;
        console.log(timeDiff);
        this._currentTime = this.getSecondsToday();
    }

    check() {
        console.log();
        console.log();
    }
}
