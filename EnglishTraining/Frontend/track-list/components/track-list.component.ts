import { VmWord } from "./models/VmWord";
import { WordsTemp } from "./wordsTemp";


export class TrackListComponent {
    // private readonly _rootScope: ng.IRootScopeService;

    private _audioPath: string = "http://wooordhunt.ru/data/sound/word/uk/mp3/";
    private _currentWord: VmWord;
    private _words: VmWord[];
    private _wordsTemp: any;
    fileToPlay: string;
    keyNextWord: number = 32;
    keyStop: number = 13;
    wordToShow: string;
    count: number = 0;

    constructor(
        // rootScope: ng.IRootScopeService,
        public $rootScope: ng.IRootScopeService,

    ) {
        // this._rootScope = rootScope;

        this.getWords();
        document.addEventListener("keydown", (e) => this.keyDownTextField(e), false);
    }

    getWords() {
        this._words = WordsTemp;
    }

    keyDownTextField(e: any) {
        var keyCode = e.keyCode;
        if (keyCode == this.keyNextWord) {
            this.wordToShow = null;
            if (this._currentWord) {
                this._words.push(this._currentWord);
            }
            this._currentWord = this._words[0];
            this.fileToPlay = this._audioPath + this._words[0].Name_en + ".mp3";
            this._words.shift();
            this.play();
            console.log("cureent word: " + this._currentWord.Name_en);
            this.logElements();
        }
        if (keyCode == this.keyStop) {
            let thirdPartOfWordsLenght: number = Math.round(this._words.length / 3);
            if (this._currentWord) {
                this._words.splice(this.getRandomNumber(thirdPartOfWordsLenght, thirdPartOfWordsLenght * 2), 0, this._currentWord);
                this.wordToShow = this._currentWord.Name_en;
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
            stringOfWords = stringOfWords + " " + w.Name_en;
        });
        console.log(stringOfWords);
        // this._rootScope.$apply();
        this.$rootScope.$apply();
    }

    getRandomNumber(min: number, max: number) {
        return Math.random() * (max - min) + min;
    }

    check() {
        console.log();
        console.log();
    }

    //   count = 0;
    myFunc() {
        this.count++;
    };
}