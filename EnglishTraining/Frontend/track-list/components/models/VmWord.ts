import { VmDictor } from "./VmDictor";

export interface VmWord {
    Name_en: string;
    Name_ru: string;
    FourDausLearnPhase: boolean;
    LearnDay: number;
    RepeatIterationNum: number;
    NextRepeatDate: string;
    DailyReapeatCountForEng: number;
    DailyReapeatCountForRus: number;
    Dictors_en?: VmDictor[];
    Dictors_ru?: VmDictor[];
}

export interface VmWordExtended extends VmWord {
    CurrentRandomLocalization: string;
}