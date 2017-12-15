import { VmDictor } from "./VmDictor";

export interface VmWord {
    // Name: object;
    Name: VmName;
    // Name_en: string;
    // Name_ru: string;
    FourDausLearnPhase: boolean;
    LearnDay: number;
    RepeatIterationNum: number;
    NextRepeatDate: string;
    DailyReapeatCountForEng: number;
    DailyReapeatCountForRus: number;
    Dictors_en?: VmDictor[];
    Dictors_ru?: VmDictor[];
}

export interface VmName {
    en: string;
    ru: string;
}

export interface VmWordExtended extends VmWord {
    CurrentRandomLocalization: string;
}