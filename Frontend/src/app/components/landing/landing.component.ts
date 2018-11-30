import { Component, ElementRef, HostListener, Inject, OnChanges, OnInit, ViewChild } from '@angular/core';
// import { Component, ElementRef, HostListener, Inject, OnChanges, OnInit, ViewChild } from '@angular/core';
import { DOCUMENT } from '@angular/platform-browser';
import { VmAudioPath } from '../../models/VmAudioPath';
import { VmWordExtended } from '../../models/VmWordExtended';
import { VmCollocation } from '../../models/VmCollocation';
import { VmWord } from '../../models/VmWord';
import { Dictionary } from '../../interfaces/Index';
import { HttpClient } from '@angular/common/http';
import { LangService } from '../../services/Lang.service';
import { ILangService } from '../../services/ILang.service';


@Component({
    selector: 'app-landing',
    templateUrl: './landing.component.html',
    styleUrls: ['./landing.component.less'],
    providers: [
        { provide: ILangService, useClass: LangService }
    ]
})

export class LandingComponent {

    private readonly hostUrl = 'http://localhost:5005';
    private readonly _apiUrl = this.hostUrl + '/main/word';
    private readonly audioPath = this.hostUrl + '/audio/';

    private readonly audioPaths: VmAudioPath = {
        'pl': this.audioPath,
        'en': this.audioPath,
        'ru': this.audioPath
    };
    private readonly _audioFormat: any = {
        'en': '.mp3',
        'ru': '.wav'
    };
    private _currentLocal: string;
    private defaultAudioPath = 'default';
    private collocationAudioPath = 'collocations';
    private _lastKeyPressedTime: number;
    private _currentWord: VmWordExtended;
    private _collocations: VmCollocation[];
    private _spentTime = 0;
    private _words: VmWordExtended[];
    private readonly KEY_NEXT_WORD = 32;
    private readonly KEY_NEXT_WORD_AUTOMODE = 33;
    private readonly KEY_STOP = 13;
    private readonly KEY_STOP_AND_PLAY = 16;
    private readonly HIGH_RATE_LEARN = 48;
    // TODO: get it from backend
    minReapeatCountPerDayFDPhase = 3;
    minReapeatCountPerDayIteration = 1;
    spentTimeToShow: string;
    wordsLoaded: number;
    autoSaveTimerPrevious: Date;
    count = 0;
    fileToPlay: string;
    wordToShow: string;
    error: string;
    mode: string;
    progress: number;
    dailyGoalRepeatCount: number;
    completedWordsCount: number;
    doneWordsTail: number;
    doneWordsPercent: number;
    sprintFinishPercent = 75;
    isDelayBeforeWordWithCollocation: boolean;
    delayBeforeWordWithCollocation = 3;
    targetLang = '';
    langList: string[] = ['pl', 'en', 'ru'];
    langForAudioPath = 'ru';
    isSavedOnAllWordsCompleted = false;
    cycle = 0;
    keyReady = true;
    // testMode = true;
    testMode = false;
    // _langService: ILangService;
    autoMode = false;
    autoModeTimer = 0;
    autoModeMinutes = 5;
    timer: any;
    wellknownMode = false;
    showSpinner = false;

    constructor(
        private http: HttpClient,
        // private langService: LangService
        // private langService: ILangService
        private todoListService: ILangService
    ) {
        // this._langService = langService;
        this.mode = 'Words';
        this.initializaeData();
    }

    initializaeData() {
        if (this.testMode) {
            this.runTest();
        } else {
            this.getTargetLang()
                .subscribe(lang => {
                    if (!lang) {
                        console.log('Target Lang should be setted');
                    }
                    this.targetLang = lang;
                    this.showSpinner = true;
                    this.getWords()
                        .subscribe((words) => {
                            if (!words) {
                                console.log('Can\'t get words');
                                return;
                            }
                            console.log(this.targetLang);
                            words.map(word => (
                                word.nextRepeatDate[this.targetLang] = new Date(word.nextRepeatDate[this.targetLang] as any)
                            ));
                            this._words = words
                                .map((word: VmWordExtended) => ({
                                    ...word
                                }));

                            // Test
                            // this._words = this._words.slice(0, 2);

                            // Add collocations into object
                            // Collocations should be separated array
                            this._collocations = [];
                            this._words.forEach(word => {
                                if (word.collocation && word.collocation.length) {
                                    word.collocation.forEach(collocation => {
                                        if (this._collocations.filter(p =>
                                            p.audioUrl === collocation.audioUrl).length === 0) {
                                            this._collocations.push({
                                                id: collocation.id,
                                                lang: collocation.lang,
                                                audioUrl: collocation.audioUrl,
                                                notUsedToday: true
                                            });
                                        }
                                    });
                                } else {
                                    word.collocation = [];
                                }
                            });

                            // TODO: findOut is it needed
                            // this._words.forEach(word => {
                            //     if (word.collocation && word.collocation[this.targetLang]) {
                            //         // what is this?
                            //         for (let i = 0; i < word.collocation[this.targetLang].length; i++) {
                            //             word.collocation[this.targetLang][i] = this._collocations.filter(z =>
                            //                 z.audioUrl == word.collocation[this.targetLang][i].audioUrl)[0];
                            //         }
                            //     }
                            // });

                            this.setNextRepeateDate();
                            this._words.sort(this.compareRandom);
                            this.wordsLoaded = this._words.length;
                            this.doneWordsTail = this._words.length - 1;

                            this.dailyGoalRepeatCount = 0;

                            const fourDaysPhaseWordNum = this._words.filter(p => p.fourDaysLearnPhase[this.targetLang] === true)
                                .length * 2 * this.minReapeatCountPerDayFDPhase;

                            const noNfourDaysPhaseWordNum = this._words.filter(p => p.fourDaysLearnPhase[this.targetLang] === false)
                                .length * 2 * this.minReapeatCountPerDayIteration;

                            this.dailyGoalRepeatCount = fourDaysPhaseWordNum + noNfourDaysPhaseWordNum;

                            this.calculateProgress();
                            this.calculateComplitedWords();
                            this.showSpinner = false;
                        });
                });
            document.addEventListener('keydown', (e) => this.keyDown(e), false);
            document.addEventListener('keyup', (e) => this.keyUp(e), false);
        }
        this.autoSaveTimerPrevious = new Date();
        this.completedWordsCount = 0;
        this.progress = 0;
        // TODO: remove it
        if (this.mode === 'Words') {
            this.checkAudio();
        }
    }

    getWords() {
        let methodUrl: string;
        // TODO: move to services
        switch (this.mode) {
            case 'Dictionary':
                methodUrl = 'dictionary';
                break;
            case 'Words':
                methodUrl = '';
                break;
            default:
                console.log('Mode should be setted');
                return null;
        }
        return this.http
            .post<VmWord[]>(`${this._apiUrl}/${methodUrl}`, { wellknownMode: true });
    }


    getTargetLang() {
        // if test use test service
        const methodUrl = 'getTargetLang';
        return this.http
            .get<string>(`${this._apiUrl}/${methodUrl}`);
    }

    setNextRepeateDate() {
        const now = new Date();
        const dateToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        this._words.forEach(word => {
            const timeDiff = Math.abs(dateToday.getTime() - word.nextRepeatDate[this.targetLang].getTime());
            const diffDays = Math.floor(timeDiff / (1000 * 3600 * 24));
            if (diffDays < 1) {
                // do nothing
            } else if (diffDays >= 1) {
                // начинается новый день повторения,
                // нужно передвинуть счетчик графика
                word = this.updateSchedule(word, dateToday, diffDays);
                // и обнулить счетчик дневных повторений
                if (this.getMaxRepeatCountFromAllLangs(word) < this.minReapeatCountPerDayFDPhase) {
                    if (word.fourDaysLearnPhase[this.targetLang]
                        && (word.learnDay[this.targetLang] > 0)) {
                        word.learnDay[this.targetLang]--;
                    }
                } else {
                    this.langList.forEach(lang => {
                        word.dailyReapeatCount[lang] = 0;
                    });
                }
            }
        });
    }

    getMaxRepeatCountFromAllLangs(word: VmWordExtended) {
        let maxValue = 0;
        this.langList.forEach(lang => {
            maxValue = word.dailyReapeatCount[lang] > maxValue ? word.dailyReapeatCount[lang] : maxValue;
        });
        return maxValue;
    }

    updateSchedule(word: VmWordExtended, dateToday: Date, diffDays: number) {
        if (word.fourDaysLearnPhase[this.targetLang]) {
            const LastRepeatingQuality = this
                .getLastRepeatingQuality(diffDays);
            switch (LastRepeatingQuality) {
                case 'good':
                    word.learnDay[this.targetLang]++;
                    if (word.learnDay[this.targetLang] >= 4) {
                        word.fourDaysLearnPhase[this.targetLang] = false;
                    }
                    break;
                case 'neutral':
                    break;
                case 'bad':
                    if (word.learnDay[this.targetLang] > 0) {
                        word.learnDay[this.targetLang]--;
                    }
                    break;
            }
            word.nextRepeatDate[this.targetLang] = dateToday;
        } else {
            if (diffDays < 1) {
                // do nothing
                // the words are being repeated this day
            } else if (diffDays >= 1) {
                // the whords are not repeated
                // set repeat day to today
                word.nextRepeatDate[this.targetLang] = dateToday;
            }
        }
        return word;
    }

    getLastRepeatingQuality(diffDays: number) {
        if (diffDays === 1) {
            return 'good';
        } else if ((diffDays > 1) && (diffDays <= 3)) {
            return 'neutral';
        } else if ((diffDays > 3)) {
            return 'bad';
        }
        return 'good';
    }

    updateWord(words: VmWord[], collocations: VmCollocation[]) {
        let methodUrl: string;
        switch (this.mode) {
            case 'Dictionary':
                methodUrl = 'updatedictionary';
                break;
            case 'Words':
                methodUrl = 'update';
                break;
            default:
                console.log('Mode should be setted');
                return null;
        }
        return this.http
            .post<string>(`${this._apiUrl}/${methodUrl}`, { words, collocations });
    }

    compareRandom(a: VmWord, b: VmWord) {
        return Math.random() - 0.5;
    }

    checkAudio() {
        return this.http
            .post<string>(`${this._apiUrl}/checkaudio`, {});
    }

    autoSave() {
        if (this.showSpinner) { return; }

        const autoSaveTimer = Math.floor(
            ((new Date()).getTime() - this.autoSaveTimerPrevious.getTime()) / 1000
        );

        const timerAmmount = 15;
        if (autoSaveTimer > timerAmmount) {
            this.calculateProgress();
            this.calculateComplitedWords();
            this.updateWord(this._words, this._collocations)
                .subscribe(p => {
                    console.log('Auto Save!!!');
                });
            this.autoSaveTimerPrevious = new Date();
        }
    }

    keyUp(e: any) {
        this.keyReady = true;
    }

    keyDown(e: any) {
        if (!this.keyReady || !this._words) {
            return;
        }
        this.keyReady = false;
        this.cycle++;
        this.autoSave();
        const keyCode = e.keyCode;
        const today = new Date;

        if (keyCode === this.KEY_NEXT_WORD || keyCode === this.KEY_NEXT_WORD_AUTOMODE) {
            if (this.pauseBeforeCollocation(this._words[0])) {
                this.playSignal();
                return;
            }
            if (this._currentWord) {
                this.returnWordToList();
            }
            if (!this._words[0].CurrentRandomLocalization) {
                this._currentLocal = this.getRandomLocal(
                    this._words[0].dailyReapeatCount,
                    this._words[0].fourDaysLearnPhase[this.targetLang]);
            } else {
                this._currentLocal = this._words[0].CurrentRandomLocalization;
                this._words[0].CurrentRandomLocalization = null;
            }

            this.wordToShow = null;
            if (this.mode === 'Dictionary') {
                this.wordToShow = this._words[0].langDictionary[this._currentLocal];
            }
            this._currentWord = this._words[0];

            this.fileToPlay = this.getFileToPlayPath(this._currentLocal, this._currentWord, true);
            console.log('cureent word: ' + this._currentWord.langDictionary[this._currentLocal]);

            this._words.shift();
            this.play();
            this.logElements();
            this.calculateProgress();
            this.firstSaveIfAllWordsCompleted();
            if (keyCode !== this.KEY_NEXT_WORD_AUTOMODE && this.autoMode) {
                this.fireUpAutoModeTimer();
            }
        }
        if ((keyCode === this.KEY_STOP && this._currentWord) ||
            (keyCode === this.KEY_STOP_AND_PLAY && this._currentWord) ||
            (keyCode === this.HIGH_RATE_LEARN && this._currentWord)) {
            this.autoMode = false;
            // if (keyCode == this.keyStop && this._currentWord) {
            let numberToSplice: number;
            const invertedLang = this.invertLanguage(this._currentLocal);

            let thirdPartOfWordsLenght: number;
            if (this.doneWordsPercent < this.sprintFinishPercent) {
                thirdPartOfWordsLenght = this._words.length / 3;
            } else {
                thirdPartOfWordsLenght = this.doneWordsTail / 3;
            }

            if (keyCode === this.HIGH_RATE_LEARN) {
                numberToSplice = this.pseudoRandomRange(4, 8, this.cycle);
            } else {
                numberToSplice = this.pseudoRandomRange(thirdPartOfWordsLenght,
                    thirdPartOfWordsLenght * 2, this.cycle);
            }
            this._currentWord.CurrentRandomLocalization = this._currentLocal;

            // TODO: get random dictor
            this.fileToPlay = this.getFileToPlayPath(invertedLang, this._currentWord, false);

            if (keyCode === this.HIGH_RATE_LEARN) {
                this.wordToShow = this._currentWord.langDictionary[this._currentLocal]
                    + ' - ' + this._currentWord.langDictionary[invertedLang];

                // Сделать переключение языка для _highRateLearn
                // Сделать включение/выключение проигрывания аудио для _highRateLearn
                this.fileToPlay = this.getFileToPlayPath(this.targetLang, this._currentWord, false);

            } else {
                this.wordToShow = this._currentWord.langDictionary[invertedLang];
            }

            this._words.splice(numberToSplice, 0, this._currentWord);

            console.log('Word ToShow = ' + this.wordToShow);

            this._currentWord = null;

            if ((keyCode === this.KEY_STOP_AND_PLAY)
                || (keyCode === this.HIGH_RATE_LEARN)) {
                this.play();
            }
            this.logElements();
        }
        this.calculateSpentTime();

    }

    getFileToPlayPath(lang: string, currentWord: VmWordExtended, useCollocation: boolean) {
        const wordTemp = currentWord.langDictionary[lang];
        let audioTypeTemp: string;
        let usernameTemp: string;
        let fileToPlay: string;
        let randNum = 0;

        // Pass collocation playing when word is new
        // and with daily repeat count < 1
        const dailyReapeatCountForLangTemp = currentWord.dailyReapeatCount[lang];

        if (useCollocation
            && lang === this.targetLang
            && currentWord.collocation
            && currentWord.collocation.length
            && this._collocations
            && (currentWord.learnDay[this.targetLang] > 0 || dailyReapeatCountForLangTemp > 1)) {

            const availableColocations = currentWord.collocation
                .filter(p => p.lang === lang
                    && p.notUsedToday === true);

            if (availableColocations.length > 0) {
                fileToPlay = availableColocations[0].audioUrl;
                availableColocations[0].notUsedToday = false;
                return fileToPlay;
            }
        }

        if (currentWord.dictors[lang].length > 1) {
            // TODO: use rather last used dictor there instead of random
            randNum = this.pseudoRandomRange(0, currentWord.dictors[lang].length - 1, this.cycle);
        }
        audioTypeTemp = currentWord.dictors[lang][randNum].audioType;
        usernameTemp = currentWord.dictors[lang][randNum].username;

        if (usernameTemp !== 'default') {
            fileToPlay = this.audioPaths[lang] +
                currentWord.langDictionary[this.langForAudioPath] + '/' + lang + '/' +
                usernameTemp + '/' + wordTemp + audioTypeTemp;
        } else {
            fileToPlay = this.audioPaths[lang] + this.defaultAudioPath + '/' +
                lang + '/' + currentWord.langDictionary[lang] + audioTypeTemp;
        }
        return fileToPlay;
    }

    play() {
        if (this.mode !== 'Words') {
            return;
        }
        this.error = null;
        const audio = new Audio(this.fileToPlay);
        audio.play()
            .catch((error) => {
                const wordNameTemp = (this._currentWord) ? this._currentWord.langDictionary[this.targetLang] : this.wordToShow;
                this.error = wordNameTemp + this._audioFormat[this._currentLocal] + ' not found';
                console.log('Error while playing ' + error);
            });
        console.log(this.fileToPlay);
    }

    playSignal() {
        const audio = new Audio('/signal/info_02_slow.mp3');
        audio.play();
    }

    logElements() {
        console.log('');
        let stringOfWords = '';
        this._words.forEach(w => {
            stringOfWords = stringOfWords + ' ' + w.langDictionary['ru'];
            if (w.CurrentRandomLocalization) {
                stringOfWords = stringOfWords + '_Has_Stored_local:' + w.CurrentRandomLocalization;
            }
        });
        console.log(stringOfWords);
        // find oud why I need it for
        // this.$rootScope.$apply();
    }

    getRandomNumber(min: number, max: number) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    pseudoRandomRange(min: number, max: number, seed: number) {
        const div = 10;
        const part = (max - min) / div;

        const randDictionary = {
            0: min + part * 0,
            1: min + part * 3,
            2: min + part * 9,
            3: min + part * 1,
            4: min + part * 4,
            5: min + part * 5,
            6: min + part * 7,
            7: min + part * 8,
            8: min + part * 2,
            9: min + part * 6
        };

        let seedtemp = seed;
        if (seedtemp > 9) {
            seedtemp = Number(seedtemp.toString()[seedtemp.toString().length - 1]);
        }

        return Math.floor(randDictionary[seedtemp]);
    }

    pseudoRandom(seed) {
        const randDictionary = {
            0: 0,
            1: 1,
            2: 0,
            3: 0,
            4: 1,
            5: 0,
            6: 1,
            7: 0,
            8: 1,
            9: 1
        };

        let seedtemp = seed;
        if (seedtemp > 9) {
            seedtemp = Number(seedtemp.toString()[seedtemp.toString().length - 1]);
        }

        return randDictionary[seedtemp];
    }

    getSumOfRepeatCountsOfNotTargetLang(dailyReapeatCount: Dictionary<number>) {
        let summ = 0;
        this.langList.filter(p => p !== this.targetLang).forEach(lang => {
            summ = summ + dailyReapeatCount[lang];
        });
        return summ;
    }

    getNotTargetLang() {
        // TODO: find out what lang to return
        if (this.targetLang === 'pl') {
            return 'en';
        } else {
            return 'pl';
        }
    }

    getRandomLocal(
        // countForEng: number,
        // countForRus: number,
        dailyReapeatCount: Dictionary<number>,
        fourDaysLearnPhase: boolean
    ) {
        const maxDiff = 2;

        const summOfRepeatCountsOfNotTargetlang = this.getSumOfRepeatCountsOfNotTargetLang(dailyReapeatCount);

        switch (fourDaysLearnPhase) {
            case true:
                if (dailyReapeatCount[this.targetLang] < this.minReapeatCountPerDayFDPhase &&
                    summOfRepeatCountsOfNotTargetlang > this.minReapeatCountPerDayFDPhase) {
                    return this.targetLang;
                } else if (dailyReapeatCount[this.targetLang] > this.minReapeatCountPerDayFDPhase &&
                    summOfRepeatCountsOfNotTargetlang < this.minReapeatCountPerDayFDPhase) {
                    return this.getNotTargetLang();
                }
                break;

            case false:
                if (dailyReapeatCount[this.targetLang] < this.minReapeatCountPerDayFDPhase &&
                    summOfRepeatCountsOfNotTargetlang >= this.minReapeatCountPerDayFDPhase) {
                    return this.targetLang;
                } else if (dailyReapeatCount[this.targetLang] >= this.minReapeatCountPerDayFDPhase &&
                    summOfRepeatCountsOfNotTargetlang < this.minReapeatCountPerDayFDPhase) {
                    return this.getNotTargetLang();
                }
                break;
        }

        const diff = dailyReapeatCount[this.targetLang] - summOfRepeatCountsOfNotTargetlang;
        if (diff >= maxDiff) {
            return this.targetLang;
        } else if (diff <= -maxDiff) {
            return this.getNotTargetLang();
        }

        const num = this.pseudoRandom(this.cycle);
        console.log(`rand = ${num}; cycle = ${this.cycle}`);

        switch (num) {
            case 0:
                return this.targetLang;
            case 1:
                return this.getNotTargetLang();
        }
    }

    invertLanguage(lang: string) {
        // get random
        return this.langList.filter(p => p !== lang)[0];
    }

    calculateSpentTime() {
        const timeNow = Math.floor((new Date()).getTime() / 1000);
        const timeDiff = timeNow - this._lastKeyPressedTime;

        if (timeDiff < 15) {
            this._spentTime = this._spentTime + timeDiff;
        }
        const min = Math.floor(this._spentTime / 60);
        const sec = this._spentTime - min * 60;
        this.spentTimeToShow = min + ' : ' + sec;
        console.log(timeDiff);
        this._lastKeyPressedTime = timeNow;
    }

    calculateComplitedWords() {
        const countForCurrentWord = this.getCountForWord(this._currentWord);

        const completedfourDaysPhaseWordNum = this._words
            .filter(p => p.fourDaysLearnPhase[this.targetLang] === true
                && p.dailyReapeatCount[this.targetLang] >= this.minReapeatCountPerDayFDPhase
                && this.getSumOfRepeatCountsOfNotTargetLang(p.dailyReapeatCount)
                // don't sure, but we shouldn't multiply this walue by not target lang amount,
                // we just should to count the sum of min repeat count for any not target lang
                >= this.minReapeatCountPerDayFDPhase).length;
        // >= this.minReapeatCountPerDayFDPhase * (this.langList.length - 1)).length;

        const completedIterationPhaseWordNum = this._words
            .filter(p => p.fourDaysLearnPhase[this.targetLang] === false
                && p.dailyReapeatCount[this.targetLang] >= this.minReapeatCountPerDayIteration
                && this.getSumOfRepeatCountsOfNotTargetLang(p.dailyReapeatCount)
                // don't sure, but we shouldn't multiply this walue by not target lang amount,
                // we just should to count the sum of min repeat count for any not target lang
                >= this.minReapeatCountPerDayIteration).length;
        // >= this.minReapeatCountPerDayIteration * (this.langList.length - 1)).length;

        this.completedWordsCount = countForCurrentWord
            + completedfourDaysPhaseWordNum + completedIterationPhaseWordNum;
    }

    firstSaveIfAllWordsCompleted() {
        if (!this.isSavedOnAllWordsCompleted &&
            this.completedWordsCount >= this.wordsLoaded) {
            this.updateWord(this._words, this._collocations)
                .subscribe(p => {
                    console.log('Auto Save!!!');
                });
            this.isSavedOnAllWordsCompleted = true;
        }
    }

    getCountForWord(word: VmWordExtended) {
        if (!word) {
            return 0;
        }

        let countForCurrentWord = 0;
        let minRepeatCountForTemp: number;

        switch (word.fourDaysLearnPhase[this.targetLang]) {
            case true:
                minRepeatCountForTemp = this.minReapeatCountPerDayFDPhase;
                break;

            case false:
                minRepeatCountForTemp = this.minReapeatCountPerDayIteration;
                break;
        }

        if (word
            && word.dailyReapeatCount[this.targetLang] >= minRepeatCountForTemp
            && this.getSumOfRepeatCountsOfNotTargetLang(word.dailyReapeatCount) >= minRepeatCountForTemp) {
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

        if (word.fourDaysLearnPhase[this.targetLang]) {
            minReapeatCountTemp = this.minReapeatCountPerDayFDPhase;
        } else {
            minReapeatCountTemp = this.minReapeatCountPerDayIteration;
        }

        if (word.dailyReapeatCount[this.targetLang] < minReapeatCountTemp) {
            progressOfWord = word.dailyReapeatCount[this.targetLang];
        } else {
            progressOfWord = minReapeatCountTemp;
        }

        if (this.getSumOfRepeatCountsOfNotTargetLang(word.dailyReapeatCount) < minReapeatCountTemp) {
            progressOfWord = progressOfWord + this.getSumOfRepeatCountsOfNotTargetLang(word.dailyReapeatCount);
        } else {
            progressOfWord = progressOfWord + minReapeatCountTemp;
        }

        return progressOfWord;
    }

    returnWordToList() {
        let minReapeatCountTemp;

        if (this._currentLocal === this.targetLang) {
            this._currentWord.dailyReapeatCount[this.targetLang]++;
        } else {
            this._currentWord.dailyReapeatCount[this._currentLocal]++;
        }

        this.doneWordsPercent = Math.round((this.completedWordsCount / this.wordsLoaded) * 100);

        if ((this.doneWordsPercent >= this.sprintFinishPercent)
            && (this.doneWordsTail > 0)) {

            if (this._currentWord.fourDaysLearnPhase[this.targetLang]) {
                minReapeatCountTemp = this.minReapeatCountPerDayFDPhase;
            } else {
                minReapeatCountTemp = this.minReapeatCountPerDayIteration;
            }

            if ((this.getSumOfRepeatCountsOfNotTargetLang(
                this._currentWord.dailyReapeatCount) >= minReapeatCountTemp)
                && (this._currentWord.dailyReapeatCount[this.targetLang] >= minReapeatCountTemp)) {
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
        if (word.collocation
            && word.collocation.length
            && word.collocation
                .filter(p => p.lang === this.targetLang)
                .filter(p => p.notUsedToday === true).length > 0) {
            this.isDelayBeforeWordWithCollocation = true;
        } else {
            this.isDelayBeforeWordWithCollocation = false;
        }
        const timeFromLastKeyPressTillNow = (new Date().getTime() / 1000)
            - this._lastKeyPressedTime;
        if (this.isDelayBeforeWordWithCollocation
            && timeFromLastKeyPressTillNow < this.delayBeforeWordWithCollocation) {
            if (!word.CurrentRandomLocalization) {
                const randLang = this.getRandomLocal(
                    word.dailyReapeatCount,
                    word.fourDaysLearnPhase[this.targetLang]);
                word.CurrentRandomLocalization = randLang;

                let dailyReapeatCountForLangTemp;
                if (randLang === this.targetLang) {
                    dailyReapeatCountForLangTemp = word.dailyReapeatCount[this.targetLang];
                } else {
                    dailyReapeatCountForLangTemp = this.getSumOfRepeatCountsOfNotTargetLang(word.dailyReapeatCount);
                }

                if (randLang === 'en'
                    && (word.learnDay[this.targetLang] > 0 || dailyReapeatCountForLangTemp > 1)) {
                    return true;
                }
            }
        }
        return false;
    }

    fireUpAutoModeTimer() {
        this.autoMode = true;
        this.setTimer();
    }

    setTimer() {
        clearTimeout(this.timer);
        this.timer = setTimeout(() => {
            this.keyUp({ keyCode: this.KEY_NEXT_WORD_AUTOMODE });
            if (!this.autoModeMinutes) {
                return;
            }
            if (this.autoModeMinutes < 3) {
                this.autoMode = false;
                this.autoModeMinutes = 5;
                this.keyUp({ keyCode: this.KEY_NEXT_WORD_AUTOMODE });
                return;
            }
            if (!this.autoMode) {
                this.keyUp({ keyCode: this.KEY_NEXT_WORD_AUTOMODE });
                return;
            }
            this.keyDown({ keyCode: this.KEY_NEXT_WORD_AUTOMODE });
            this.setTimer();
        }, this.autoModeMinutes * 1000);
    }

    runTest() {

    }

    switchToWellknownMode() {
        // this.wellknownMode = !this.wellknownMode;
        this.wellknownMode = true;
        this.resetData();
        this.initializaeData();
    }

    resetData() {
        this._words = [];
        this._collocations = [];
        this.progress = 0;
        this._currentWord = null;
    }
}
