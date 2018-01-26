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
    private _audioFormat: any = {
        "en": ".mp3",
        "ru": ".wav"
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
    wordsLoaded: number;
    autoSaveTimerPrevious: number;
    count: number = 0;
    fileToPlay: string;
    wordToShow: string;
    error: string;
    mode: string;

    constructor(
        // http: ng.IHttpService,
        private $http: ng.IHttpService,

        public $rootScope: ng.IRootScopeService,
        // apiService: ApiService
    ) {
        // this._http = http;
        this._apiUrl = "/main/word";
        // this._apiSrv = apiService;
        this.mode = "Words";
        this.getWords()
            .then((words) => {
                words.map(word => (
                    word.nextRepeatDate = new Date(word.nextRepeatDate as any)
                ));
                this._words = words
                    .map((word: VmWordExtended) => ({
                        ...word,
                        Name: {
                            en: word.name_en,
                            ru: word.name_ru
                        }
                    }));
                this.setNextRepeateDate();
                this.wordsLoaded = this._words.length;
            });
        document.addEventListener("keydown", (e) => this.keyDownTextField(e), false);
        this.autoSaveTimerPrevious = this.getSecondsToday();
        if (this.mode === "Words") { 
            this.checkAudio();
        }
    }

    getWords() {
        let methodUrl: string;
        // TODO: move to services
        switch (this.mode) { 
            case "Dictionary":
                methodUrl = "dictionary";
                break
            case "Words":
                methodUrl = "";
                break
            default:
                console.log("Mode should be setted");    
            return null;
        }
        return this.$http
            .get<VmWord[]>(`${this._apiUrl}/${methodUrl}`)
            .then(response => response.data);
    }

    setNextRepeateDate() {
        let now = new Date();
        var dateToday = new Date(now.getFullYear(), now.getMonth(), now.getDate())
        this._words.forEach(word => {
            if (word.nextRepeatDate === dateToday) {
                // do nothing
            } else if (word.nextRepeatDate < dateToday) { 
                // начинается новый день повторения,
                // нужно передвинуть счетчик графика
                word = this.updateSchedule(word);
                // и обнулить счетчик дневных повторений
                word.dailyReapeatCountForRus = 0;
                word.dailyReapeatCountForEng = 0;
            }
        });
    }

    updateSchedule(word: VmWordExtended) {
        let now = new Date();
        var dateToday = new Date(now.getFullYear(), now.getMonth(), now.getDate())
        if (word.fourDaysLearnPhase) {
            let LastRepeatingQuality = this
                .getLastRepeatingQuality(dateToday, word.nextRepeatDate);
            switch (LastRepeatingQuality) {
                case "good":
                    word.learnDay++;
                    if (word.learnDay == 4) { 
                        word.fourDaysLearnPhase = false;
                    }
                    break;
                case "neutral":
                    break;
                case "bad":
                    if (word.learnDay > 0) {
                        word.learnDay--;
                    }    
                    break;
            }
            word.nextRepeatDate = dateToday;
        } else {
            if (word.nextRepeatDate == dateToday) {
                // do nothing
                // the words are being repeated this day
            } else if ((word.dailyReapeatCountForEng != 0)
                        && (word.dailyReapeatCountForRus != 0)    
                        && (word.nextRepeatDate < dateToday)) {
                // the whords are not repeated
                // set repeat day to today
                word.nextRepeatDate = dateToday;
            } else if ((word.dailyReapeatCountForEng == 0)
                        && (word.dailyReapeatCountForRus == 0)
                        && (word.nextRepeatDate < dateToday)) {
                // whord are repeated
                // set repeat day to next iteration day
                word.repeatIterationNum++;
                word.nextRepeatDate = dateToday;
                let days = 7;
                word.nextRepeatDate.setDate(word.nextRepeatDate.getDate()
                + (days * word.repeatIterationNum));
            }
        };
        return word;
    }

    getLastRepeatingQuality(dayToday: Date, lastRepeatDay: Date) {
        const timeDiff = Math.abs(dayToday.getTime() - lastRepeatDay.getTime());
        const diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));
        if (diffDays == 1) {
            return "good";
        } else if ((diffDays > 1) && (diffDays <= 3)) { 
            return "neutral"
        } else if ((diffDays > 3)) {
            return "bad"
        }
        return "good";
    }

    updateWord(words: VmWord[]) {
        let methodUrl: string;
        switch (this.mode) {
            case "Dictionary":
                methodUrl = "updatedictionary";
                break
            case "Words":
                methodUrl = "update";
                break
            default:
                console.log("Mode should be setted");
                return null;
        }
        return this.$http
            .post<string>(`${this._apiUrl}/${methodUrl}`, words)
            .then(response => response.data);
    }

    checkAudio() {
        return this.$http
            .post<string>(`${this._apiUrl}/checkaudio`, {});
    }

    autoSave() {
        let autoSaveTimer = this.getSecondsToday() - this.autoSaveTimerPrevious;
        const timerAmmount: number = 15;
        if (autoSaveTimer > timerAmmount) {
            this.updateWord(this._words);
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
            if (this.mode === "Dictionary") {
                this.wordToShow = this._words[0].Name[this._currentLocal];
            }

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

            this.fileToPlay = this._audioPath[this._currentLocal] + this._words[0].Name[this._currentLocal] + this._audioFormat[this._currentLocal];

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

            if (keyCode == this._highRateLearn) {
                this.wordToShow = this._currentWord.Name[this._currentLocal]
                    + " - " + this._currentWord.Name[invertedLang];
            } else {
                this.wordToShow = this._currentWord.Name[invertedLang];
            }

            this._words.splice(numberToSplice, 0, this._currentWord);

            console.log("Word ToShow = " + this.wordToShow);

            this.fileToPlay = this._audioPath[invertedLang] +
                this._currentWord.Name[invertedLang] + this._audioFormat[invertedLang];

            this._currentWord = null;

            if (keyCode == 16) {
                this.play();
            }
            this.logElements();
        }
    }

    play() {
        if (this.mode != "Words") { 
            return;
        }
        this.error = null;        
        var audio = new Audio(this.fileToPlay);
        audio.play()
            .catch((error) => {
                let wordNameTemp = (this._currentWord) ? this._currentWord.name_ru : this.wordToShow;
                this.error = wordNameTemp + this._audioFormat[this._currentLocal] + " not found";
                console.log("Error while playing " + error);
            });
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
