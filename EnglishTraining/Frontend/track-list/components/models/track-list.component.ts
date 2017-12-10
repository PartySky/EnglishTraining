export class TrackListComponent {
    private _words: string[];
    private _audioPath: string = "http://wooordhunt.ru/data/sound/word/uk/mp3/";
    fileToPlay: string;
    keyNextWord: number = 32;
    keyStop: number = 13;

    constructor() {
        this.getWords();
        document.addEventListener("keydown", (e) => this.keyDownTextField(e), false);
    }


    keyDownTextField(e: any) {
        var keyCode = e.keyCode;
        if (keyCode == this.keyNextWord) {
            let tempWord = this._words[0];
            this._words.shift();
            this._words.push(tempWord);
            this.fileToPlay = this._audioPath + this._words[0] + ".mp3";
            this.play();
            this.logElements();
        }
        if (keyCode == this.keyStop) {
            let tempWord = this._words[0];
            let halfWordsLenght: number = Math.round(this._words.length / 2);
            this._words.shift();
            this._words.splice(this.getRandomNumber(1, halfWordsLenght), 0, tempWord);
            this.fileToPlay = this._audioPath + "wrong" + ".mp3";
            this.play();
            this.logElements();
        }
    }


    getWords() { 
        this._words = [
            "good",
            "bad",
            "suffuse",
            "rile",
            "bodily",
            "dishonest"
        ]
    }

    play() {
        var audio = new Audio(this.fileToPlay);
        audio.play();
    }

    logElements() { 
        console.log("");
        let stringOfWords: string;
        this._words.forEach(w => {
            stringOfWords = stringOfWords + " " + w;
        });
        console.log(stringOfWords);
    }

    getRandomNumber(min: number, max: number) {
        return Math.random() * (max - min) + min;
    }

    check() {
        console.log();
        console.log();
    }
}