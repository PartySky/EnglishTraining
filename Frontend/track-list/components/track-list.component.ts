import { VmWord } from "./models/VmWord";
import { VmWordExtended } from "./models/VmWordExtended";
import { VmAudioPath } from "./models/VmAudioPath";
import { name } from "../track-list.module";
import { fail } from "assert";

export class TrackListComponent {
    // private readonly _http: ng.IHttpService;
    private readonly _apiUrl: string;
    // private readonly _apiSrv: ApiService;
    private _audioPath: VmAudioPath = {
        "en": "./audio/",
        "ru": "./audio/"
    }
    private _audioFormat: any = {
        "en": ".mp3",
        "ru": ".wav"
    }
    private _currentLocal: string;
    private _engLocal: string = "en";
    private _rusLocal: string = "en";
    private defaultAudioPath: string = "default";
    private _currentTime: number;
    private _currentWord: VmWordExtended;
    private _spentTime: number = 0;
    private _words: VmWordExtended[];
    private _keyNextWord: number = 32;
    private _keyStop: number = 13;
    private _keyStopAndPlay: number = 16;
    private _highRateLearn: number = 48;
    // TODO: get it from backend
    minReapeatCountPerDayFDPhase: number = 3;
    minReapeatCountPerDayIteration: number = 1;
    spentTimeToShow: string;
    wordsLoaded: number;
    autoSaveTimerPrevious: Date;
    count: number = 0;
    fileToPlay: string;
    wordToShow: string;
    error: string;
    mode: string;
    progress: number;
    dailyGoalRepeatCount: number;
    completedWordsCount: number;
    doneWordsTail: number;
    doneWordsPercent: number;
    sprintFinishPercent: number = 75;

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
                this._words.sort(this.compareRandom);
                this.wordsLoaded = this._words.length;
                this.doneWordsTail = this._words.length - 1;

                this.dailyGoalRepeatCount = 0;

                let fourDaysPhaseWordNum = this._words.filter(p => p.fourDaysLearnPhase == true)
                    .length * 2 * this.minReapeatCountPerDayFDPhase;
                
                let noNfourDaysPhaseWordNum = this._words.filter(p => p.fourDaysLearnPhase == false)
                    .length * 2 * this.minReapeatCountPerDayIteration;
                
                this.dailyGoalRepeatCount = fourDaysPhaseWordNum + noNfourDaysPhaseWordNum;  

                this.calculateComplitedWords();
            });
        document.addEventListener("keydown", (e) => this.keyDownTextField(e), false);
        this.autoSaveTimerPrevious = new Date();
        this.completedWordsCount = 0;
        this.progress = 0;
        // TODO: remove it
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
            const timeDiff = Math.abs(dateToday.getTime() - word.nextRepeatDate.getTime());
            const diffDays = Math.floor(timeDiff / (1000 * 3600 * 24));
            if (diffDays < 1) {
                // do nothing
            } else if (diffDays >= 1) {
                // начинается новый день повторения,
                // нужно передвинуть счетчик графика
                word = this.updateSchedule(word, dateToday, diffDays);
                // и обнулить счетчик дневных повторений
                if ((word.dailyReapeatCountForRus < this.minReapeatCountPerDayFDPhase)
                    && (word.dailyReapeatCountForEng < this.minReapeatCountPerDayFDPhase)) {
                    if (word.fourDaysLearnPhase
                        && (word.learnDay > 0)) {
                        word.learnDay--;
                    }
                } else {
                    word.dailyReapeatCountForRus = 0;
                    word.dailyReapeatCountForEng = 0;
                }
            }
        });
    }

    updateSchedule(word: VmWordExtended, dateToday: Date, diffDays: number) {
        if (word.fourDaysLearnPhase) {
            let LastRepeatingQuality = this
                .getLastRepeatingQuality(diffDays);
            switch (LastRepeatingQuality) {
                case "good":
                    word.learnDay++;
                    if (word.learnDay >= 4) {
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
            if (diffDays < 1) {
                // do nothing
                // the words are being repeated this day
            } else if (diffDays >= 1) {
                // the whords are not repeated
                // set repeat day to today
                word.nextRepeatDate = dateToday;
            }
        };
        return word;
    }

    getLastRepeatingQuality(diffDays: number) {
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

    compareRandom(a: VmWord, b: VmWord) {
        return Math.random() - 0.5;
    }

    checkAudio() {
        return this.$http
            .post<string>(`${this._apiUrl}/checkaudio`, {});
    }

    autoSave() {
        let autoSaveTimer = Math.floor(
            ((new Date()).getTime() - this.autoSaveTimerPrevious.getTime()) / 1000
        );

        const timerAmmount: number = 15;
        if (autoSaveTimer > timerAmmount) {
            this.calculateProgress();
            this.calculateComplitedWords()
            this.updateWord(this._words);
            console.log("Auto Save!!!");
            this.autoSaveTimerPrevious = new Date();
        }
    }

    keyDownTextField(e: any) {
        this.autoSave();
        let keyCode = e.keyCode;
        let today = new Date;
        this.calculateSpentTime();
        if (keyCode === this._keyNextWord) {
            if (this._currentWord) {
                this.returnWordToList();
            }
            if (!this._words[0].CurrentRandomLocalization) {
                this._currentLocal = this.getRandomLocal(
                    this._words[0].dailyReapeatCountForEng,
                    this._words[0].dailyReapeatCountForRus,
                    this._words[0].fourDaysLearnPhase);
            } else {
                this._currentLocal = this._words[0].CurrentRandomLocalization;
                this._words[0].CurrentRandomLocalization = null;
            }
            this.wordToShow = null;
            if (this.mode === "Dictionary") {
                this.wordToShow = this._words[0].Name[this._currentLocal];
            }
            this._currentWord = this._words[0];

            this.fileToPlay = this.getFileToPlayPath(this._currentLocal, this._words[0]);

            console.log("cureent word: " + this._words[0].Name[this._currentLocal]);

            this._words.shift();
            this.play();
            this.logElements();
        }
        if ((keyCode === this._keyStop && this._currentWord) ||
            (keyCode === this._keyStopAndPlay && this._currentWord) ||
            (keyCode === this._highRateLearn && this._currentWord)) {
            // if (keyCode == this.keyStop && this._currentWord) {
            let numberToSplice: number;
            let invertedLang = this.invertLanguage(this._currentLocal);

            let thirdPartOfWordsLenght: number;
            if (this.doneWordsPercent < this.sprintFinishPercent) {
                thirdPartOfWordsLenght = this._words.length / 3;
            } else {
                thirdPartOfWordsLenght = this.doneWordsTail / 3;
            }

            if (keyCode === this._highRateLearn) {
                numberToSplice = this.getRandomNumber(4, 8);
            } else {
                numberToSplice = this.getRandomNumber(thirdPartOfWordsLenght,
                    thirdPartOfWordsLenght * 2);
            }
            this._currentWord.CurrentRandomLocalization = this._currentLocal;

            // TODO: get random dictor
            this.fileToPlay = this.getFileToPlayPath(invertedLang, this._currentWord);

            if (keyCode == this._highRateLearn) {
                this.wordToShow = this._currentWord.Name[this._currentLocal]
                    + " - " + this._currentWord.Name[invertedLang];

                // Сделать переключение языка для _highRateLearn
                // Сделать включение/выключение проигрывания аудио для _highRateLearn
                this.fileToPlay = this.getFileToPlayPath(this._engLocal, this._currentWord);

            } else {
                this.wordToShow = this._currentWord.Name[invertedLang];
            }

            this._words.splice(numberToSplice, 0, this._currentWord);

            console.log("Word ToShow = " + this.wordToShow);

            this._currentWord = null;

            if ((keyCode == this._keyStopAndPlay)
                || (keyCode == this._highRateLearn)) {
                this.play();
            }
            this.logElements();
        }
    }

    getFileToPlayPath(lang: string, currentWord: VmWordExtended) {
        let wordTemp = currentWord.Name[lang];
        let audioTypeTemp: string;
        let usernameTemp: string;
        let fileToPlay: string;
        let randNum = 0;

        if (lang == "en") {
            if (currentWord.dictors_en.length > 1) {
                randNum = this.getRandomNumber(0, currentWord.dictors_en.length - 1);
            }
            audioTypeTemp = currentWord.dictors_en[randNum].audioType;
            usernameTemp = currentWord.dictors_en[randNum].username
        } else {
            if (currentWord.dictors_ru.length > 1) {
                randNum = this.getRandomNumber(0, currentWord.dictors_ru.length - 1);
            }
            audioTypeTemp = currentWord.dictors_ru[randNum].audioType;
            usernameTemp = currentWord.dictors_ru[randNum].username
        }

        if (usernameTemp != "default") {
            fileToPlay = this._audioPath[lang] +
                currentWord.name_ru + "/" + lang + "/" +
                usernameTemp + "/" + wordTemp + audioTypeTemp;
        } else {
            fileToPlay = this._audioPath[lang] + this.defaultAudioPath + "/" +
                lang + "/" + currentWord.Name[lang] + audioTypeTemp;
        }
        return fileToPlay;
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
        return Math.round(Math.random() * (max - min) + min);
    }

    getRandomLocal(
        countForEng: number,
        countForRus: number,
        fourDaysLearnPhase: boolean
    ) {
        var maxDiff = 2;

        switch (fourDaysLearnPhase) {
            case true:
                if ((countForEng > this.minReapeatCountPerDayFDPhase)
                    && (countForRus < this.minReapeatCountPerDayFDPhase)) {
                    return "ru";
                } else if ((countForEng < this.minReapeatCountPerDayFDPhase)
                    && (countForRus > this.minReapeatCountPerDayFDPhase)) {
                    return "en";
                };
                break;
            
            case false:
                if ((countForEng >= this.minReapeatCountPerDayIteration)
                    && (countForRus < this.minReapeatCountPerDayIteration)) {
                    return "ru";
                } else if ((countForEng <= this.minReapeatCountPerDayIteration)
                    && (countForRus > this.minReapeatCountPerDayIteration)) {
                    return "en";
                };
                break;
        }

        

        var diff = countForEng - countForRus;
        if (diff >= maxDiff) {
            return "ru";
        }
        else if (diff <= -maxDiff) {
            return "en";
        }

        let num = this.getRandomNumber(0, 1);
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

    calculateSpentTime() {
        const timeNow = Math.floor((new Date()).getTime() / 1000);
        const timeDiff = timeNow - this._currentTime;

        if (timeDiff < 15) {
            this._spentTime = this._spentTime + timeDiff;
        }
        let min = Math.floor(this._spentTime / 60);
        let sec = this._spentTime - min * 60;
        this.spentTimeToShow = min + " : " + sec;
        console.log(timeDiff);
        this._currentTime = timeNow;
    }


    calculateComplitedWords() {
        let countForCurrentWord = this.getCountForWord(this._currentWord);

        const completedfourDaysPhaseWordNum = this._words
            .filter(p => p.fourDaysLearnPhase == true
                && p.dailyReapeatCountForEng >= this.minReapeatCountPerDayFDPhase
                && p.dailyReapeatCountForRus >= this.minReapeatCountPerDayFDPhase).length;

        const completedIterationPhaseWordNum = this._words
            .filter(p => p.fourDaysLearnPhase == false
                && p.dailyReapeatCountForEng >= this.minReapeatCountPerDayIteration
                && p.dailyReapeatCountForRus >= this.minReapeatCountPerDayIteration).length;

        this.completedWordsCount = countForCurrentWord
            + completedfourDaysPhaseWordNum + completedIterationPhaseWordNum;
    }

    getCountForWord(word: VmWordExtended) { 
        if (!word) { 
            return 0;
        }

        let countForCurrentWord = 0;
        let minRepeatCountForTemp: number;

        switch (word.fourDaysLearnPhase) {
            case true:
                minRepeatCountForTemp = this.minReapeatCountPerDayFDPhase;
                break;

            case false:
                minRepeatCountForTemp = this.minReapeatCountPerDayIteration;
                break;
        }

        if (word
            && word.dailyReapeatCountForEng >= minRepeatCountForTemp
            && word.dailyReapeatCountForRus >= minRepeatCountForTemp) {
            countForCurrentWord = 1;
        }

        return countForCurrentWord;
    }

    calculateProgress() {
        this.progress = 0;

        if (this._currentWord) {
            this.progress = this.progress + this._getProgressOfWord(this._currentWord);
        }

        this._words.forEach(word => {
            this.progress = this.progress + this._getProgressOfWord(word);
        });
    }

    _getProgressOfWord(word: VmWordExtended) {
        let progressOfWord = 0;

        switch (word.fourDaysLearnPhase) {
            case true:
                if (word.dailyReapeatCountForEng < this.minReapeatCountPerDayFDPhase) {
                    progressOfWord = word.dailyReapeatCountForEng;
                } else {
                    progressOfWord = this.minReapeatCountPerDayFDPhase;
                }

                if (word.dailyReapeatCountForRus < this.minReapeatCountPerDayFDPhase) {
                    progressOfWord = progressOfWord + word.dailyReapeatCountForRus;
                } else {
                    progressOfWord = progressOfWord + this.minReapeatCountPerDayFDPhase;
                }
                break;

            case false:
                if (word.dailyReapeatCountForEng < this.minReapeatCountPerDayIteration) {
                    progressOfWord = word.dailyReapeatCountForEng;
                } else {
                    progressOfWord = this.minReapeatCountPerDayIteration;
                }

                if (word.dailyReapeatCountForRus < this.minReapeatCountPerDayIteration) {
                    progressOfWord = progressOfWord + word.dailyReapeatCountForRus;
                } else {
                    progressOfWord = progressOfWord + this.minReapeatCountPerDayIteration;
                }
                break;
        }

        return progressOfWord;
    }

    returnWordToList() {
        if (this._currentLocal == "en") {
            this._currentWord.dailyReapeatCountForEng++;
        } else {
            this._currentWord.dailyReapeatCountForRus++;
        }

        this.doneWordsPercent = Math.round((this.completedWordsCount / this.wordsLoaded) * 100);

        if ((this.doneWordsPercent >= this.sprintFinishPercent)
            && (this.doneWordsTail > 0)) {

            switch (this._currentWord.fourDaysLearnPhase) {
                case true:
                    if ((this._currentWord.dailyReapeatCountForRus >= this.minReapeatCountPerDayFDPhase)
                        && (this._currentWord.dailyReapeatCountForEng >= this.minReapeatCountPerDayFDPhase)) {
                        this._words.push(this._currentWord);
                        this.doneWordsTail--;
                    } else {
                        this._words.splice(this.doneWordsTail, 0, this._currentWord);
                    }
                    break;

                case false:
                    if ((this._currentWord.dailyReapeatCountForRus >= this.minReapeatCountPerDayIteration)
                        && (this._currentWord.dailyReapeatCountForEng >= this.minReapeatCountPerDayIteration)) {
                        this._words.push(this._currentWord);
                        this.doneWordsTail--;
                    } else {
                        this._words.splice(this.doneWordsTail, 0, this._currentWord);
                    }
                    break;
            }

        } else {
            this._words.push(this._currentWord);
        }
    }

    check() {
        console.log();
        console.log();
    }
}
