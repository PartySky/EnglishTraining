import { VmWord } from "./models/VmWord";
import { VmWordExtended } from "./models/VmWordExtended";
import { VmAudioPath } from "./models/VmAudioPath";
import { WordsTemp } from "./wordsTemp";

export class TrackListComponent {
    private _audioPath: VmAudioPath = {
        "en": "http://wooordhunt.ru/data/sound/word/uk/mp3/",
        "ru": "./audio/"
    }
    private _currentLocal: string;
    private _currentTime: number;
    private _currentWord: VmWordExtended;
    private _spentTime: number = 0;
    private _words: VmWordExtended[];
    private _wordsTemp: any;
    private _keyNextWord: number = 32;
    private _keyStop: number = 13;
    private _highRateLearn: number = 48;
    spentTimeToShow: string;
    count: number = 0;
    fileToPlay: string;
    wordToShow: string;

    constructor(
        public $rootScope: ng.IRootScopeService,

    ) {
        this.getWords();
        document.addEventListener("keydown", (e) => this.keyDownTextField(e), false);
    }

    getWords() {
        this._words = WordsTemp;
    }

    keyDownTextField(e: any) {
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
