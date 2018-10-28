import { VmWord } from "./models/VmWord";
import { VmWordExtended } from "./models/VmWordExtended";
import { VmAudioPath } from "./models/VmAudioPath";
import { name } from "../track-list.module";
import { fail } from "assert";
import { VmCollocation } from "./models/VmCollocation";

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
    private collocationAudioPath: string = "collocations";
    private _lastKeyPressedTime: number;
    private _currentWord: VmWordExtended;
    private _collocations: VmCollocation[];
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
    isDelayBeforeWordWithCollocation: boolean;
    delayBeforeWordWithCollocation: number = 3;
    targetLang = '';
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
        this.getTargetLang()
            .then(lang => {
                if (!lang) {
                    debugger;
                    console.log('Target Lang should be setted');
                }
                this.targetLang = lang;
                this.getWords()
                    .then((words) => {
                        debugger;
                        words.map(word => (
                            word.nextRepeatDate_Old = new Date(word.nextRepeatDate_Old as any)
                        ));
                        this._words = words
                            .map((word: VmWordExtended) => ({
                                ...word,
                                Name: {
                                    en: word.name_en_Old,
                                    ru: word.name_ru_Old
                                }
                            }));

                        // Add collocations into object
                        this._collocations = [];
                        this._words.forEach(word => {
                            word.collocation_Old.forEach(collocation => {
                                if (this._collocations.filter(p =>
                                    p.audioUrl == collocation.audioUrl).length == 0) {

                                    this._collocations.push({
                                        id: collocation.id,
                                        lang: collocation.lang,
                                        audioUrl: collocation.audioUrl,
                                        notUsedToday: true
                                    });
                                }
                            });
                        });

                        this._words.forEach(word => {
                            for (let i = 0; i < word.collocation_Old.length; i++) {
                                word.collocation_Old[i] = this._collocations.filter(z =>
                                    z.audioUrl == word.collocation_Old[i].audioUrl)[0];
                            }
                        });

                        this.setNextRepeateDate();
                        this._words.sort(this.compareRandom);
                        this.wordsLoaded = this._words.length;
                        this.doneWordsTail = this._words.length - 1;

                        this.dailyGoalRepeatCount = 0;

                        let fourDaysPhaseWordNum = this._words.filter(p => p.fourDaysLearnPhase_Old == true)
                            .length * 2 * this.minReapeatCountPerDayFDPhase;

                        let noNfourDaysPhaseWordNum = this._words.filter(p => p.fourDaysLearnPhase_Old == false)
                            .length * 2 * this.minReapeatCountPerDayIteration;

                        this.dailyGoalRepeatCount = fourDaysPhaseWordNum + noNfourDaysPhaseWordNum;

                        this.calculateComplitedWords();
                    });
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


    getTargetLang() {
        const methodUrl = "getTargetLang";
        return this.$http
            .get<string>(`${this._apiUrl}/${methodUrl}`)
            .then(response => response.data);
    }

    setNextRepeateDate() {
        let now = new Date();
        var dateToday = new Date(now.getFullYear(), now.getMonth(), now.getDate())
        this._words.forEach(word => {
            const timeDiff = Math.abs(dateToday.getTime() - word.nextRepeatDate_Old.getTime());
            const diffDays = Math.floor(timeDiff / (1000 * 3600 * 24));
            if (diffDays < 1) {
                // do nothing
            } else if (diffDays >= 1) {
                // начинается новый день повторения,
                // нужно передвинуть счетчик графика
                word = this.updateSchedule(word, dateToday, diffDays);
                // и обнулить счетчик дневных повторений
                if ((word.dailyReapeatCountForRus_Old < this.minReapeatCountPerDayFDPhase)
                    && (word.dailyReapeatCountForEng_Old < this.minReapeatCountPerDayFDPhase)) {
                    if (word.fourDaysLearnPhase_Old
                        && (word.learnDay_Old > 0)) {
                        word.learnDay_Old--;
                    }
                } else {
                    word.dailyReapeatCountForRus_Old = 0;
                    word.dailyReapeatCountForEng_Old = 0;
                }
            }
        });
    }

    updateSchedule(word: VmWordExtended, dateToday: Date, diffDays: number) {
        if (word.fourDaysLearnPhase_Old) {
            let LastRepeatingQuality = this
                .getLastRepeatingQuality(diffDays);
            switch (LastRepeatingQuality) {
                case "good":
                    word.learnDay_Old++;
                    if (word.learnDay_Old >= 4) {
                        word.fourDaysLearnPhase_Old = false;
                    }
                    break;
                case "neutral":
                    break;
                case "bad":
                    if (word.learnDay_Old > 0) {
                        word.learnDay_Old--;
                    }
                    break;
            }
            word.nextRepeatDate_Old = dateToday;
        } else {
            if (diffDays < 1) {
                // do nothing
                // the words are being repeated this day
            } else if (diffDays >= 1) {
                // the whords are not repeated
                // set repeat day to today
                word.nextRepeatDate_Old = dateToday;
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

    updateWord(words: VmWord[], collocations: VmCollocation[]) {
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
            .post<string>(`${this._apiUrl}/${methodUrl}`, { words, collocations })
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
            this.updateWord(this._words, this._collocations);
            console.log("Auto Save!!!");
            this.autoSaveTimerPrevious = new Date();
        }
    }

    keyDownTextField(e: any) {
        this.autoSave();
        let keyCode = e.keyCode;
        let today = new Date;

        if (keyCode === this._keyNextWord) {
            if (this.pauseBeforeCollocation(this._words[0])) {
                this.playSignal();
                return;
            }
            if (this._currentWord) {
                this.returnWordToList();
            }
            if (!this._words[0].CurrentRandomLocalization) {
                this._currentLocal = this.getRandomLocal(
                    this._words[0].dailyReapeatCountForEng_Old,
                    this._words[0].dailyReapeatCountForRus_Old,
                    this._words[0].fourDaysLearnPhase_Old);
            } else {
                this._currentLocal = this._words[0].CurrentRandomLocalization;
                this._words[0].CurrentRandomLocalization = null;
            }

            this.wordToShow = null;
            if (this.mode === "Dictionary") {
                this.wordToShow = this._words[0].Name[this._currentLocal];
            }
            this._currentWord = this._words[0];

            this.fileToPlay = this.getFileToPlayPath(this._currentLocal, this._currentWord, true);

            console.log("cureent word: " + this._currentWord.Name[this._currentLocal]);

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
            this.fileToPlay = this.getFileToPlayPath(invertedLang, this._currentWord, false);

            if (keyCode == this._highRateLearn) {
                this.wordToShow = this._currentWord.Name[this._currentLocal]
                    + " - " + this._currentWord.Name[invertedLang];

                // Сделать переключение языка для _highRateLearn
                // Сделать включение/выключение проигрывания аудио для _highRateLearn
                this.fileToPlay = this.getFileToPlayPath(this._engLocal, this._currentWord, false);

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
        this.calculateSpentTime();

    }

    getFileToPlayPath(lang: string, currentWord: VmWordExtended, useCollocation: boolean) {
        let wordTemp = currentWord.Name[lang];
        let audioTypeTemp: string;
        let usernameTemp: string;
        let fileToPlay: string;
        let randNum = 0;

        // Pass collocation playing when word is new 
        // and with daily repeat count < 1
        let dailyReapeatCountForLangTemp;
        if (lang == "en") {
            dailyReapeatCountForLangTemp = currentWord.dailyReapeatCountForEng_Old;
        } else {
            dailyReapeatCountForLangTemp = currentWord.dailyReapeatCountForRus_Old;
        }

        if (useCollocation
            && currentWord.collocation_Old
            && this._collocations
            && (currentWord.learnDay_Old > 0 || dailyReapeatCountForLangTemp > 1)) {

            let availableColocations = currentWord.collocation_Old
                .filter(p => p.lang == lang
                    && p.notUsedToday == true);

            if (availableColocations.length > 0) {
                fileToPlay = availableColocations[0].audioUrl;
                availableColocations[0].notUsedToday = false;
                return fileToPlay;
            }
        }

        if (lang == "en") {
            if (currentWord.dictors_en_Old.length > 1) {
                randNum = this.getRandomNumber(0, currentWord.dictors_en_Old.length - 1);
            }
            audioTypeTemp = currentWord.dictors_en_Old[randNum].audioType;
            usernameTemp = currentWord.dictors_en_Old[randNum].username
        } else {
            if (currentWord.dictors_ru_Old.length > 1) {
                randNum = this.getRandomNumber(0, currentWord.dictors_ru_Old.length - 1);
            }
            audioTypeTemp = currentWord.dictors_ru_Old[randNum].audioType;
            usernameTemp = currentWord.dictors_ru_Old[randNum].username
        }

        if (usernameTemp != "default") {
            fileToPlay = this._audioPath[lang] +
                currentWord.name_ru_Old + "/" + lang + "/" +
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
                let wordNameTemp = (this._currentWord) ? this._currentWord.name_ru_Old : this.wordToShow;
                this.error = wordNameTemp + this._audioFormat[this._currentLocal] + " not found";
                console.log("Error while playing " + error);
            });
        console.log(this.fileToPlay);
    }

    playSignal() {
        var audio = new Audio("/signal/info_02_slow.mp3");
        audio.play()
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
        return Math.floor(Math.random() * (max - min + 1)) + min;
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
        const timeDiff = timeNow - this._lastKeyPressedTime;

        if (timeDiff < 15) {
            this._spentTime = this._spentTime + timeDiff;
        }
        let min = Math.floor(this._spentTime / 60);
        let sec = this._spentTime - min * 60;
        this.spentTimeToShow = min + " : " + sec;
        console.log(timeDiff);
        this._lastKeyPressedTime = timeNow;
    }


    calculateComplitedWords() {
        let countForCurrentWord = this.getCountForWord(this._currentWord);

        const completedfourDaysPhaseWordNum = this._words
            .filter(p => p.fourDaysLearnPhase_Old == true
                && p.dailyReapeatCountForEng_Old >= this.minReapeatCountPerDayFDPhase
                && p.dailyReapeatCountForRus_Old >= this.minReapeatCountPerDayFDPhase).length;

        const completedIterationPhaseWordNum = this._words
            .filter(p => p.fourDaysLearnPhase_Old == false
                && p.dailyReapeatCountForEng_Old >= this.minReapeatCountPerDayIteration
                && p.dailyReapeatCountForRus_Old >= this.minReapeatCountPerDayIteration).length;

        this.completedWordsCount = countForCurrentWord
            + completedfourDaysPhaseWordNum + completedIterationPhaseWordNum;
    }

    getCountForWord(word: VmWordExtended) {
        if (!word) {
            return 0;
        }

        let countForCurrentWord = 0;
        let minRepeatCountForTemp: number;

        switch (word.fourDaysLearnPhase_Old) {
            case true:
                minRepeatCountForTemp = this.minReapeatCountPerDayFDPhase;
                break;

            case false:
                minRepeatCountForTemp = this.minReapeatCountPerDayIteration;
                break;
        }

        if (word
            && word.dailyReapeatCountForEng_Old >= minRepeatCountForTemp
            && word.dailyReapeatCountForRus_Old >= minRepeatCountForTemp) {
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
        let minReapeatCountTemp;

        if (word.fourDaysLearnPhase_Old) {
            minReapeatCountTemp = this.minReapeatCountPerDayFDPhase;
        } else {
            minReapeatCountTemp = this.minReapeatCountPerDayIteration;
        }

        if (word.dailyReapeatCountForEng_Old < minReapeatCountTemp) {
            progressOfWord = word.dailyReapeatCountForEng_Old;
        } else {
            progressOfWord = minReapeatCountTemp;
        }

        if (word.dailyReapeatCountForRus_Old < minReapeatCountTemp) {
            progressOfWord = progressOfWord + word.dailyReapeatCountForRus_Old;
        } else {
            progressOfWord = progressOfWord + minReapeatCountTemp;
        }

        return progressOfWord;
    }

    returnWordToList() {
        let minReapeatCountTemp;

        if (this._currentLocal == "en") {
            this._currentWord.dailyReapeatCountForEng_Old++;
        } else {
            this._currentWord.dailyReapeatCountForRus_Old++;
        }

        this.doneWordsPercent = Math.round((this.completedWordsCount / this.wordsLoaded) * 100);

        if ((this.doneWordsPercent >= this.sprintFinishPercent)
            && (this.doneWordsTail > 0)) {

            if (this._currentWord.fourDaysLearnPhase_Old) {
                minReapeatCountTemp = this.minReapeatCountPerDayFDPhase;
            } else {
                minReapeatCountTemp = this.minReapeatCountPerDayIteration;
            }

            if ((this._currentWord.dailyReapeatCountForRus_Old >= minReapeatCountTemp)
                && (this._currentWord.dailyReapeatCountForEng_Old >= minReapeatCountTemp)) {
                this._words.push(this._currentWord);
                this.doneWordsTail--;
            } else {
                this._words.splice(this.doneWordsTail, 0, this._currentWord);
            }
        } else {
            this._words.push(this._currentWord);
        }
    }

    pauseBeforeCollocation(word: VmWordExtended) {
        if (word.collocation_Old.length > 0
            && word.collocation_Old.filter(p => p.notUsedToday == true).length > 0) {
            this.isDelayBeforeWordWithCollocation = true;
        } else {
            this.isDelayBeforeWordWithCollocation = false;
        }
        let timeFromLastKeyPressTillNow = (new Date().getTime() / 1000)
            - this._lastKeyPressedTime;
        if (this.isDelayBeforeWordWithCollocation
            && timeFromLastKeyPressTillNow < this.delayBeforeWordWithCollocation) {
            if (!word.CurrentRandomLocalization) {
                let randLang = this.getRandomLocal(
                    word.dailyReapeatCountForEng_Old,
                    word.dailyReapeatCountForRus_Old,
                    word.fourDaysLearnPhase_Old);
                word.CurrentRandomLocalization = randLang;

                let dailyReapeatCountForLangTemp;
                if (randLang == "en") {
                    dailyReapeatCountForLangTemp = word.dailyReapeatCountForEng_Old;
                } else {
                    dailyReapeatCountForLangTemp = word.dailyReapeatCountForRus_Old;
                }

                if (randLang == "en"
                    && (word.learnDay_Old > 0 || dailyReapeatCountForLangTemp > 1)) {
                    return true;
                }
            }
        }
        return false;
    }
}
