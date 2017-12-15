import { VmWord, VmWordExtended } from "./models/VmWord";
import { WordsTemp } from "./wordsTemp";

export class TrackListComponent {
    private _audioPath: string = "http://wooordhunt.ru/data/sound/word/uk/mp3/";
    private _currentWord: VmWordExtended;
    private _currentLocal: string;
    private _words: VmWordExtended[];
    // private _words: any[];
    private _wordsTemp: any;
    private _randomLocalizedName: string;
    fileToPlay: string;
    keyNextWord: number = 32;
    keyStop: number = 13;
    wordToShow: string;
    count: number = 0;

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
        if (!this._words[0].CurrentRandomLocalization) {
            this._currentLocal = this.getRandomLocal();
        } else {
            this._currentLocal = this._words[0].CurrentRandomLocalization;
            this._words[0].CurrentRandomLocalization = null;
        }
        var keyCode = e.keyCode;
        if (keyCode == this.keyNextWord) {
            this.wordToShow = null;
            if (this._currentWord) {
                this._words.push(this._currentWord);
            }
            this._currentWord = this._words[0];
            this._currentWord.CurrentRandomLocalization = this._currentLocal;

            // this.fileToPlay = this._audioPath + this._words[0].Name["en"] + ".mp3";

            this.fileToPlay = this._audioPath + this._words[0].Name[this._currentLocal] + ".mp3";

            this._words.shift();
            this.play();
            // console.log("cureent word: " + this._currentWord.Name_en);
            // console.log("cureent word: " + this._currentWord.Name_ru);
            console.log("cureent word: " + this._randomLocalizedName);
            this.logElements();
        }
        if (keyCode == this.keyStop) {
            this._words[0].CurrentRandomLocalization = this._currentLocal;
            let thirdPartOfWordsLenght: number = Math.round(this._words.length / 3);
            if (this._currentWord) {
                this._words.splice(this.getRandomNumber(thirdPartOfWordsLenght, thirdPartOfWordsLenght * 2), 0, this._currentWord);
                // this.wordToShow = this._currentWord.Name_en;
                // this.wordToShow = this._currentWord.Name_ru;
                this.wordToShow = this._randomLocalizedName;
                console.log("Word ToShow = " + this.wordToShow);
            }
            this._currentWord = null;

            this.fileToPlay = this._audioPath + "wrong" + ".mp3";
            this.play();
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
            stringOfWords = stringOfWords + " " + w.Name_ru;
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

    check() {
        console.log();
        console.log();
    }
}
